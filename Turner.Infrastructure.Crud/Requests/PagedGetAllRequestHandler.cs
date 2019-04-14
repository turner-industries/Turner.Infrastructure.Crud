using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal class PagedGetAllRequestHandler<TRequest, TEntity, TOut>
        : CrudRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, PagedGetAllResult<TOut>>
        where TEntity : class
        where TRequest : IPagedGetAllRequest<TEntity, TOut>
    {
        protected readonly RequestOptions Options;

        public PagedGetAllRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public async Task<Response<PagedGetAllResult<TOut>>> HandleAsync(TRequest request)
        {
            PagedGetAllResult<TOut> result = null;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                try
                {
                    await request.RunRequestHooks(RequestConfig, ct).Configure();
                    
                    var entities = Context
                        .Set<TEntity>()
                        .FilterWith(request, RequestConfig)
                        .SortWith(request, RequestConfig);
                    
                    var totalItemCount = await entities.CountAsync(ct).Configure();
                    ct.ThrowIfCancellationRequested();

                    var pageSize = request.PageSize < 1 ? totalItemCount : request.PageSize;
                    var totalPageCount = totalItemCount == 0 ? 1 : (totalItemCount + pageSize - 1) / pageSize;
                    var pageNumber = Math.Max(1, Math.Min(request.PageNumber, totalPageCount));
                    var startIndex = (pageNumber - 1) * pageSize;

                    entities = entities.Skip(startIndex).Take(pageSize);

                    var (tOuts, found) = await FindEntities(request, entities, ct).Configure();
                    if (!found && RequestConfig.ErrorConfig.FailedToFindInGetAllIsError)
                    {
                        var errorResult = new PagedGetAllResult<TOut>(tOuts, pageNumber, pageSize, totalPageCount, totalItemCount);
                        var error = new FailedToFindError(request, typeof(TEntity), errorResult);

                        return ErrorDispatcher.Dispatch<PagedGetAllResult<TOut>>(error);
                    }

                    var items = await request.RunResultHooks(RequestConfig, tOuts, ct).Configure();
                    
                    result = new PagedGetAllResult<TOut>(items, pageNumber, pageSize, totalPageCount, totalItemCount);
                }
                catch (Exception e) when (RequestFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<PagedGetAllResult<TOut>>(RequestFailedError.From(request, e, result));
                }
                catch (Exception e) when (RequestCanceledError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<PagedGetAllResult<TOut>>(RequestCanceledError.From(request, e, result));
                }
                catch (Exception e) when (HookFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<PagedGetAllResult<TOut>>(HookFailedError.From(request, e, result));
                }
            }

            return result.AsResponse();
        }

        private async Task<(TOut[], bool)> FindEntities(TRequest request, IQueryable<TEntity> entities, CancellationToken ct)
        {
            var result = Array.Empty<TOut>();
            
            if (Options.UseProjection)
            {
                result = await entities.ProjectToArrayAsync<TEntity, TOut>(ct).Configure();
                ct.ThrowIfCancellationRequested();
            }
            else
            {
                var resultEntities = await entities.ToArrayAsync(ct).Configure();
                ct.ThrowIfCancellationRequested();

                await request.RunEntityHooks<TEntity>(RequestConfig, entities, ct).Configure();

                result = await resultEntities.CreateResults<TEntity, TOut>(RequestConfig, ct).Configure();
                ct.ThrowIfCancellationRequested();
            }

            var found = result.Length > 0;

            if (!found)
            {
                var defaultValue = RequestConfig.GetDefaultFor<TEntity>();
                if (defaultValue != null)
                {
                    result = new[]
                    {
                        await defaultValue
                            .CreateResult<TEntity, TOut>(RequestConfig, ct)
                            .Configure()
                    };
                }
            }

            ct.ThrowIfCancellationRequested();

            return (result, found);
        }
    }
}

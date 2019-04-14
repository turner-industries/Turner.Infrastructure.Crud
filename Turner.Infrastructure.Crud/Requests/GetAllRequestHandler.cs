using System;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal class GetAllRequestHandler<TRequest, TEntity, TOut>
        : CrudRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, GetAllResult<TOut>>
        where TEntity : class
        where TRequest : IGetAllRequest<TEntity, TOut>
    {
        protected readonly RequestOptions Options;

        public GetAllRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public async Task<Response<GetAllResult<TOut>>> HandleAsync(TRequest request)
        {
            GetAllResult<TOut> result = null;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                try
                {
                    await request.RunRequestHooks(RequestConfig, ct).Configure();

                    var (tOuts, found) = await FindEntities(request, ct).Configure();
                    if (!found && RequestConfig.ErrorConfig.FailedToFindInGetAllIsError)
                    {
                        var errorResult = new GetAllResult<TOut>(tOuts);
                        var error = new FailedToFindError(request, typeof(TEntity), errorResult);

                        return ErrorDispatcher.Dispatch<GetAllResult<TOut>>(error);
                    }
                    
                    var items = await request.RunResultHooks(RequestConfig, tOuts, ct).Configure();

                    result = new GetAllResult<TOut>(items);
                }
                catch (Exception e) when (RequestFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<GetAllResult<TOut>>(RequestFailedError.From(request, e, result));
                }
                catch (Exception e) when (RequestCanceledError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<GetAllResult<TOut>>(RequestCanceledError.From(request, e, result));
                }
                catch (Exception e) when (HookFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<GetAllResult<TOut>>(HookFailedError.From(request, e, result));
                }
            }

            return result.AsResponse();
        }

        private async Task<(TOut[], bool)> FindEntities(TRequest request, CancellationToken ct)
        {
            var found = false;
            var result = Array.Empty<TOut>();

            var entities = Context.Set<TEntity>()
                .FilterWith(request, RequestConfig)
                .SortWith(request, RequestConfig);

            if (Options.UseProjection)
            {
                result = await entities.ProjectToArrayAsync<TEntity, TOut>(ct).Configure();
                ct.ThrowIfCancellationRequested();
                found = result.Length > 0;

                if (!found)
                {
                    var defaultEntity = RequestConfig.GetDefaultFor<TEntity>();
                    if (defaultEntity != null)
                    {
                        result = new TOut[]
                        {
                            await defaultEntity
                                .CreateResult<TEntity, TOut>(RequestConfig, ct)
                                .Configure()
                        };
                    }
                }
            }
            else
            {
                var resultEntities = await entities.ToArrayAsync(ct).Configure();
                ct.ThrowIfCancellationRequested();
                found = result.Length > 0;

                if (!found)
                {
                    var defaultEntity = RequestConfig.GetDefaultFor<TEntity>();
                    if (defaultEntity != null)
                        resultEntities = new TEntity[] { RequestConfig.GetDefaultFor<TEntity>() };
                }

                await request.RunEntityHooks<TEntity>(RequestConfig, entities, ct).Configure();

                result = await resultEntities
                    .CreateResults<TEntity, TOut>(RequestConfig, ct)
                    .Configure();
            }

            ct.ThrowIfCancellationRequested();

            return (result, found);
        }
    }
}

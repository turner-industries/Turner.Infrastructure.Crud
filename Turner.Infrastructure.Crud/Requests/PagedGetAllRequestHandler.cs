using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
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
            PagedGetAllResult<TOut> result;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                List<TOut> items;

                await request.RunRequestHooks(RequestConfig.GetRequestHooks(), ct).Configure();

                var entities = Context
                    .Set<TEntity>()
                    .FilterWith(request, RequestConfig.GetFiltersFor<TEntity>())
                    .SortWith(request, RequestConfig.GetSorterFor<TEntity>());

                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();
                
                var totalItemCount = await entities.CountAsync(ct).Configure();
                ct.ThrowIfCancellationRequested();

                var pageSize = request.PageSize < 1 ? totalItemCount : request.PageSize;
                var totalPageCount = totalItemCount == 0 ? 1 : (totalItemCount + pageSize - 1) / pageSize;
                var pageNumber = Math.Max(1, Math.Min(request.PageNumber, totalPageCount));
                var startIndex = (pageNumber - 1) * pageSize;

                entities = entities
                    .Skip(startIndex)
                    .Take(pageSize);

                if (Options.UseProjection)
                {
                    items = await entities
                        .ProjectTo<TOut>()
                        .ToListAsync(ct)
                        .Configure();

                    ct.ThrowIfCancellationRequested();
                }
                else
                {
                    var resultEntities = await entities.ToListAsync(ct).Configure();
                    ct.ThrowIfCancellationRequested();

                    items = new List<TOut>(await Task.WhenAll(resultEntities.Select(x => transform(x, ct))).Configure());
                    ct.ThrowIfCancellationRequested();
                }

                if (items.Count == 0)
                {
                    var defaultValue = RequestConfig.GetDefaultFor<TEntity>();
                    if (defaultValue != null)
                        items.Add(await transform(defaultValue, ct).Configure());

                    if (RequestConfig.ErrorConfig.FailedToFindInGetAllIsError)
                    {
                        var errorResult = new PagedGetAllResult<TOut>(items, pageNumber, pageSize, totalPageCount, totalItemCount);
                        var error = new FailedToFindError(request, typeof(TEntity), errorResult);

                        return ErrorDispatcher.Dispatch<PagedGetAllResult<TOut>>(error);
                    }

                    ct.ThrowIfCancellationRequested();
                }

                items = await request.RunResultHooks(RequestConfig.GetResultHooks(), items, ct);

                result = new PagedGetAllResult<TOut>(items, pageNumber, pageSize, totalPageCount, totalItemCount);
            }

            return result.AsResponse();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
#pragma warning disable 0618

    internal class PagedGetRequestHandler<TRequest, TEntity, TOut>
        : CrudRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, PagedGetResult<TOut>>
        where TEntity : class
        where TRequest : IPagedGetRequest<TEntity, TOut>
    {
        protected readonly RequestOptions Options;

        public PagedGetRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public async Task<Response<PagedGetResult<TOut>>> HandleAsync(TRequest request)
        {
            PagedGetResult<TOut> result;
            var failedToFind = false;

            try
            {
                var requestHooks = RequestConfig.GetRequestHooks(request);
                foreach (var hook in requestHooks)
                    await hook.Run(request).Configure();

                var selector = RequestConfig.GetSelectorFor<TEntity>();
                var entities = Context
                    .EntitySet<TEntity>()
                    .AsQueryable();

                foreach (var filter in RequestConfig.GetFiltersFor<TEntity>())
                    entities = filter.Filter(request, entities);

                var sorter = RequestConfig.GetSorterFor<TEntity>();
                entities = sorter?.Sort(request, entities) ?? entities;

                var totalItemCount = await Context.CountAsync(entities).Configure();
                var pageSize = request.PageSize < 1 ? totalItemCount : request.PageSize;
                var totalPageCount = totalItemCount == 0 ? 1 : (totalItemCount + pageSize - 1) / pageSize;

                var allItems = await Context.ToArrayAsync(entities).Configure();
                var item = allItems
                    .Select((e, i) => new { Item = e, Index = i })
                    .SingleOrDefault(x => selector.Get<TEntity>()(request).Compile()(x.Item));

                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();
                var resultItems = new List<TOut>();
                var pageNumber = 0;

                if (item != null)
                {
                    var pageItems = allItems.Skip(item.Index / pageSize * pageSize).Take(pageSize);
                    resultItems = new List<TOut>(await Task.WhenAll(pageItems.Select(transform)));

                    pageNumber = 1 + (item.Index / pageSize);
                }
                else
                {
                    failedToFind = true;

                    var defaultValue = RequestConfig.GetDefaultFor<TEntity>();
                    if (defaultValue != null)
                        resultItems.Add(await transform(defaultValue).Configure());

                    pageNumber = 0;
                }

                result = new PagedGetResult<TOut>
                {
                    Items = resultItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    PageCount = totalPageCount,
                    TotalItemCount = totalItemCount
                };
            }
            catch (CrudRequestFailedException e)
            {
                var error = new RequestFailedError(request, e);
                return ErrorDispatcher.Dispatch<PagedGetResult<TOut>>(error);
            }

            if (failedToFind && RequestConfig.ErrorConfig.FailedToFindInFindIsError)
            {
                var error = new FailedToFindError(request, typeof(TEntity), result);
                return ErrorDispatcher.Dispatch<PagedGetResult<TOut>>(error);
            }

            return result.AsResponse();
        }
    }

    #pragma warning restore 0618
}

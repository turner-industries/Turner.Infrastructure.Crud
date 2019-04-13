using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    #pragma warning disable 0618

    internal class PagedFindRequestHandler<TRequest, TEntity, TOut>
        : CrudRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, PagedFindResult<TOut>>
        where TEntity : class
        where TRequest : IPagedFindRequest<TEntity, TOut>
    {
        protected readonly RequestOptions Options;

        public PagedFindRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public async Task<Response<PagedFindResult<TOut>>> HandleAsync(TRequest request)
        {
            PagedFindResult<TOut> result;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;
                var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();
                var failedToFind = false;

                try
                {
                    await request.RunRequestHooks(RequestConfig.GetRequestHooks(), ct).Configure();

                    var entities = Context.Set<TEntity>()
                        .FilterWith(request, RequestConfig.GetFiltersFor<TEntity>())
                        .SortWith(request, RequestConfig.GetSorterFor<TEntity>());
                    
                    var totalItemCount = await entities.CountAsync(ct).Configure();
                    ct.ThrowIfCancellationRequested();

                    var pageSize = request.PageSize < 1 ? totalItemCount : request.PageSize;
                    var totalPageCount = totalItemCount == 0 ? 1 : (totalItemCount + pageSize - 1) / pageSize;

                    var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>()(request).Compile();

                    var item = (await entities.ToArrayAsync(ct).Configure())
                        .Select((e, i) => new { Item = e, Index = i })
                        .SingleOrDefault(x => selector(x.Item));

                    ct.ThrowIfCancellationRequested();

                    if (item != null)
                    {
                        var resultItem = await transform(item.Item, ct).Configure();
                        ct.ThrowIfCancellationRequested();

                        resultItem = await request.RunResultHooks(RequestConfig.GetResultHooks(), resultItem, ct);

                        result = new PagedFindResult<TOut>
                        {
                            Item = resultItem,
                            PageNumber = 1 + (item.Index / pageSize),
                            PageSize = pageSize,
                            PageCount = totalPageCount,
                            TotalItemCount = totalItemCount
                        };
                    }
                    else
                    {
                        failedToFind = true;

                        var resultItem = await transform(RequestConfig.GetDefaultFor<TEntity>(), ct).Configure();
                        ct.ThrowIfCancellationRequested();

                        resultItem = await request.RunResultHooks(RequestConfig.GetResultHooks(), resultItem, ct);

                        result = new PagedFindResult<TOut>
                        {
                            Item = resultItem,
                            PageNumber = 0,
                            PageSize = pageSize,
                            PageCount = totalPageCount,
                            TotalItemCount = totalItemCount
                        };
                    }
                }
                catch (CrudRequestFailedException e)
                {
                    var error = new RequestFailedError(request, e);
                    return ErrorDispatcher.Dispatch<PagedFindResult<TOut>>(error);
                }

                if (failedToFind && RequestConfig.ErrorConfig.FailedToFindInFindIsError)
                {
                    var error = new FailedToFindError(request, typeof(TEntity), result);
                    return ErrorDispatcher.Dispatch<PagedFindResult<TOut>>(error);
                }
            }

            return result.AsResponse();
        }
    }

    #pragma warning restore 0618
}

using AutoMapper;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;
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
                
                var item = (await Context.ToArrayAsync(entities).Configure())
                    .Select((e, i) => new { Item = e, Index = i })
                    .SingleOrDefault(x => selector.Get<TEntity>()(request).Compile()(x.Item));
                
                if (item != null)
                {
                    result = new PagedFindResult<TOut>
                    {
                        Item = Mapper.Map<TOut>(item.Item),
                        PageNumber = 1 + (item.Index / pageSize),
                        PageSize = pageSize,
                        PageCount = totalPageCount,
                        TotalItemCount = totalItemCount
                    };
                }
                else
                { 
                    failedToFind = true;

                    result = new PagedFindResult<TOut>
                    {
                        Item = Mapper.Map<TOut>(RequestConfig.GetDefaultFor<TEntity>()),
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

            return result.AsResponse();
        }
    }

    #pragma warning restore 0618
}

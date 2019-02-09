using AutoMapper.QueryableExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
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
            List<TOut> items;
            var transform = RequestConfig.GetResultCreatorFor<TEntity, TOut>();

            var entities = Context
                .EntitySet<TEntity>()
                .AsQueryable();

            foreach (var filter in RequestConfig.GetFiltersFor<TEntity>())
                entities = filter.Filter(request, entities);

            var totalItemCount = await Context.CountAsync(entities).Configure();
            
            var sorter = RequestConfig.GetSorterFor<TEntity>();
            entities = sorter?.Sort(request, entities) ?? entities;

            var pageSize = request.PageSize < 1 ? totalItemCount : request.PageSize;
            var totalPageCount = totalItemCount == 0 ? 1 : (totalItemCount + pageSize - 1) / pageSize;
            var pageNumber = Math.Max(1, Math.Min(request.PageNumber, totalPageCount));
            var startIndex = (pageNumber - 1) * pageSize;

            entities = entities
                .Skip(startIndex)
                .Take(pageSize);

            if (Options.UseProjection)
            {
                items = await Context.ToListAsync(entities.ProjectTo<TOut>()).Configure();
            }
            else
            {
                var resultEntities = await Context.ToListAsync(entities).Configure();
                items = new List<TOut>(await Task.WhenAll(resultEntities.Select(transform)).Configure());
            }

            if (items.Count == 0)
            {
                var defaultValue = RequestConfig.GetDefaultFor<TEntity>();
                if (defaultValue != null)
                    items.Add(await transform(defaultValue).Configure());

                if (RequestConfig.ErrorConfig.FailedToFindInGetAllIsError)
                {
                    var errorResult = new PagedGetAllResult<TOut>(items, pageNumber, pageSize, totalPageCount, totalItemCount);
                    var error = new FailedToFindError(request, typeof(TEntity), errorResult);

                    return ErrorDispatcher.Dispatch<PagedGetAllResult<TOut>>(error);
                }
            }

            var resultHooks = RequestConfig.GetResultHooks(request);
            foreach (var hook in resultHooks)
                for (var i = 0; i < items.Count; ++i)
                    items[i] = (TOut)await hook.Run(request, items[i]).Configure();

            var result = new PagedGetAllResult<TOut>(items, pageNumber, pageSize, totalPageCount, totalItemCount);

            return result.AsResponse();
        }
    }
}

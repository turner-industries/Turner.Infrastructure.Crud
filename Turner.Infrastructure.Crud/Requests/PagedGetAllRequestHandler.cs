using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal class PagedGetAllRequestHandler<TRequest, TEntity, TOut>
        : CrudRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, PagedGetAllResult<TOut>>
        where TEntity : class
        where TRequest : IPagedGetAllRequest<TEntity, TOut>
    {
        protected readonly IGetAllAlgorithm Algorithm;
        protected readonly RequestOptions Options;

        public PagedGetAllRequestHandler(DbContext context,
            CrudConfigManager profileManager,
            IGetAllAlgorithm algorithm)
            : base(context, profileManager)
        {
            Algorithm = algorithm;
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public async Task<Response<PagedGetAllResult<TOut>>> HandleAsync(TRequest request)
        {
            List<TOut> items;

            var entities = Algorithm
                .GetEntities<TEntity>(Context)
                .AsQueryable();

            foreach (var filter in RequestConfig.GetFiltersFor<TEntity>())
                entities = filter.Filter(request, entities);

            var totalItemCount = await entities.CountAsync();
            
            var sorter = RequestConfig.GetSorterFor<TEntity>();
            entities = sorter?.Sort(request, entities) ?? entities;

            var pageSize = request.PageSize < 1 ? totalItemCount : request.PageSize;
            var totalPageCount = totalItemCount == 0 ? 1 : (totalItemCount + pageSize - 1) / pageSize;
            var pageNumber = Math.Max(1, Math.Min(request.PageNumber, totalPageCount));
            var startIndex = (pageNumber - 1) * pageSize;

            entities = entities
                .Skip(startIndex)
                .Take(pageSize);

            items = await StandardGetAllAlgorithm
                .ProjectResultItems<TEntity, TOut>(Options, entities);

            if (items.Count == 0)
            {
                var defaultValue = RequestConfig.GetDefaultFor<TEntity>();
                if (defaultValue != null)
                    items.Add(Mapper.Map<TOut>(defaultValue));

                if (RequestConfig.ErrorConfig.FailedToFindInGetAllIsError)
                {
                    var errorResult = new PagedGetAllResult<TOut>(items, pageNumber, pageSize, totalPageCount, totalItemCount);
                    var error = new FailedToFindError(request, typeof(TEntity), errorResult);

                    return ErrorDispatcher.Dispatch<PagedGetAllResult<TOut>>(error);
                }
            }

            var result = new PagedGetAllResult<TOut>(items, pageNumber, pageSize, totalPageCount, totalItemCount);

            return result.AsResponse();
        }
    }
}

using System.Collections.Generic;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;
// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IPagedGetAllRequest : IPagedRequest, IGetAllRequest
    {
    }

    public interface IPagedGetAllRequest<TEntity, TOut> : IPagedGetAllRequest, IRequest<PagedGetAllResult<TOut>>
        where TEntity : class
    {
    }
    
    [DoNotValidate]
    public class PagedGetAllRequest<TEntity, TOut> : IPagedGetAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }

    public class PagedGetAllResult<TOut>
    {
        public List<TOut> Items { get; }

        public int PageNumber { get; }

        public int PageSize { get; }

        public int PageCount { get; }

        public int TotalItemCount { get; }

        public PagedGetAllResult(List<TOut> items, int pageNumber, int pageSize, int pageCount, int totalCount)
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            PageCount = pageCount;
            TotalItemCount = totalCount;
        }
    }
}

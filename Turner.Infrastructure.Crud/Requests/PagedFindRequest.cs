using System;
using Turner.Infrastructure.Mediator;
// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
#pragma warning disable 0618

    [Obsolete("Linq does not currently support positional queries. PagedFindRequest may cause a large result set to be created.")]
    public interface IPagedFindRequest : ICrudRequest
    {
        int PageSize { get; set; }
    }

    [Obsolete("Linq does not currently support positional queries. PagedFindRequest may cause a large result set to be created.")]
    public interface IPagedFindRequest<TEntity, TOut> : IPagedFindRequest, IRequest<PagedFindResult<TOut>>
        where TEntity : class
    {
    }
    
    public class PagedFindResult<TOut>
    {
        public TOut Item { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int PageCount { get; set; }

        public int TotalItemCount { get; set; }
    }

    #pragma warning restore 0618
}

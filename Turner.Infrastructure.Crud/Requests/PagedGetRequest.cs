using System;
using System.Collections.Generic;
using Turner.Infrastructure.Mediator;
// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
#pragma warning disable 0618

    [Obsolete("Linq does not currently support positional queries. PagedGetRequest may cause a large result set to be created.")]
    public interface IPagedGetRequest : ICrudRequest
    {
        int PageSize { get; set; }
    }

    [Obsolete("Linq does not currently support positional queries. PagedGetRequest may cause a large result set to be created.")]
    public interface IPagedGetRequest<TEntity, TOut> : IPagedGetRequest, IRequest<PagedGetResult<TOut>>
        where TEntity : class
    {
    }

    public class PagedGetResult<TOut>
    {
        public List<TOut> Items { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int PageCount { get; set; }

        public int TotalItemCount { get; set; }
    }

#pragma warning restore 0618
}

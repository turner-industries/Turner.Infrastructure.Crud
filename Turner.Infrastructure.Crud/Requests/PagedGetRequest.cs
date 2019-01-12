using System;
using System.Collections.Generic;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;

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

    [Obsolete("Linq does not currently support positional queries. PagedGetRequest may cause a large result set to be created.")]
    [DoNotValidate]
    public class PagedGetRequest<TEntity, TKey, TOut> : IPagedGetRequest<TEntity, TOut>
        where TEntity : class
    {
        public PagedGetRequest(TKey key, int pageSize = 10)
        {
            Key = key;
            PageSize = pageSize;
        }

        public TKey Key { get; }

        public int PageSize { get; set; }
    }

    public class PagedGetProfile<TEntity, TKey, TOut>
        : CrudRequestProfile<PagedGetRequest<TEntity, TKey, TOut>>
        where TEntity : class
    {
        public PagedGetProfile()
        {
            ForEntity<TEntity>().WithRequestKey(request => request.Key);
        }
    }

    [Obsolete("Linq does not currently support positional queries. PagedGetRequest may cause a large result set to be created.")]
    [DoNotValidate]
    public class PagedGetByIdRequest<TEntity, TOut> 
        : PagedGetRequest<TEntity, int, TOut>
        where TEntity : class
    {
        public PagedGetByIdRequest(int id, int pageSize = 10)
            : base(id, pageSize)
        {
        }
    }

    public class PagedGetByIdRequestProfile<TEntity, TOut>
        : CrudRequestProfile<PagedGetByIdRequest<TEntity, TOut>>
        where TEntity : class
    {
        public PagedGetByIdRequestProfile()
        {
            ForEntity<TEntity>().WithEntityKey("Id");
        }
    }

    [Obsolete("Linq does not currently support positional queries. PagedGetRequest may cause a large result set to be created.")]
    [DoNotValidate]
    public class PagedGetByGuidRequest<TEntity, TOut> 
        : PagedGetRequest<TEntity, Guid, TOut>
        where TEntity : class
    {
        public PagedGetByGuidRequest(Guid guid, int pageSize = 10)
            : base(guid, pageSize)
        {
        }
    }

    public class PagedGetByGuidRequestProfile<TEntity, TOut>
        : CrudRequestProfile<PagedGetByGuidRequest<TEntity, TOut>>
        where TEntity : class
    {
        public PagedGetByGuidRequestProfile()
        {
            ForEntity<TEntity>().WithEntityKey("Guid");
        }
    }

#pragma warning restore 0618
}

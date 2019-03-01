using System;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    #pragma warning disable 0618

    [Obsolete("Linq does not currently support positional queries. PagedGetRequest may cause a large result set to be created.")]
    [MaybeValidate]
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

    public class PagedGetRequestProfile<TEntity, TKey, TOut>
        : CrudRequestProfile<PagedGetRequest<TEntity, TKey, TOut>>
        where TEntity : class
    {
        public PagedGetRequestProfile()
        {
            ForEntity<TEntity>().WithRequestKey(request => request.Key);
        }
    }

    [Obsolete("Linq does not currently support positional queries. PagedGetRequest may cause a large result set to be created.")]
    [MaybeValidate]
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
    [MaybeValidate]
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

    [Obsolete("Linq does not currently support positional queries. PagedGetRequest may cause a large result set to be created.")]
    [MaybeValidate]
    public class PagedGetByNameRequest<TEntity, TOut>
        : PagedGetRequest<TEntity, string, TOut>
        where TEntity : class
    {
        public PagedGetByNameRequest(string name, int pageSize = 10)
            : base(name, pageSize)
        {
        }
    }

    public class PagedGetByNameRequestProfile<TEntity, TOut>
        : CrudRequestProfile<PagedGetByNameRequest<TEntity, TOut>>
        where TEntity : class
    {
        public PagedGetByNameRequestProfile()
        {
            ForEntity<TEntity>().WithEntityKey("Name");
        }
    }

    #pragma warning restore 0618
}

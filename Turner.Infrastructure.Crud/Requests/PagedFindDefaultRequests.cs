﻿using System;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    #pragma warning disable 0618

    [Obsolete("Linq does not currently support positional queries. PagedFindRequest may cause a large result set to be created.")]
    [MaybeValidate]
    public class PagedFindRequest<TEntity, TKey, TOut> : IPagedFindRequest<TEntity, TOut>
        where TEntity : class
    {
        public PagedFindRequest(TKey key, int pageSize = 10)
        {
            Key = key;
            PageSize = pageSize;
        }

        public TKey Key { get; }

        public int PageSize { get; set; }
    }

    public class PagedFindRequestProfile<TEntity, TKey, TOut>
        : CrudRequestProfile<PagedFindRequest<TEntity, TKey, TOut>>
        where TEntity : class
    {
        public PagedFindRequestProfile()
        {
            ForEntity<TEntity>()
                .UseRequestKey(request => request.Key);
        }
    }

    [Obsolete("Linq does not currently support positional queries. PagedFindRequest may cause a large result set to be created.")]
    [MaybeValidate]
    public class PagedFindByIdRequest<TEntity, TOut> : PagedFindRequest<TEntity, int, TOut>
        where TEntity : class
    {
        public PagedFindByIdRequest(int id, int pageSize = 10)
            : base(id, pageSize)
        {
        }
    }

    public class PagedFindByIdRequestProfile<TEntity, TOut>
        : CrudRequestProfile<PagedFindByIdRequest<TEntity, TOut>>
        where TEntity : class
    {
        public PagedFindByIdRequestProfile()
        {
            ForEntity<TEntity>().UseEntityKey("Id");
        }
    }

    [Obsolete("Linq does not currently support positional queries. PagedFindRequest may cause a large result set to be created.")]
    [MaybeValidate]
    public class PagedFindByGuidRequest<TEntity, TOut> : PagedFindRequest<TEntity, Guid, TOut>
        where TEntity : class
    {
        public PagedFindByGuidRequest(Guid guid, int pageSize = 10)
            : base(guid, pageSize)
        {
        }
    }

    public class PagedFindByGuidRequestProfile<TEntity, TOut>
        : CrudRequestProfile<PagedFindByGuidRequest<TEntity, TOut>>
        where TEntity : class
    {
        public PagedFindByGuidRequestProfile()
        {
            ForEntity<TEntity>().UseEntityKey("Guid");
        }
    }

    [Obsolete("Linq does not currently support positional queries. PagedFindRequest may cause a large result set to be created.")]
    [MaybeValidate]
    public class PagedFindByNameRequest<TEntity, TOut> : PagedFindRequest<TEntity, string, TOut>
        where TEntity : class
    {
        public PagedFindByNameRequest(string name, int pageSize = 10)
            : base(name, pageSize)
        {
        }
    }

    public class PagedFindByNameRequestProfile<TEntity, TOut>
        : CrudRequestProfile<PagedFindByNameRequest<TEntity, TOut>>
        where TEntity : class
    {
        public PagedFindByNameRequestProfile()
        {
            ForEntity<TEntity>().UseEntityKey("Name");
        }
    }

    #pragma warning restore 0618
}

﻿using System;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;

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

    [Obsolete("Linq does not currently support positional queries. PagedFindRequest may cause a large result set to be created.")]
    [DoNotValidate]
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

    [Obsolete("Linq does not currently support positional queries. PagedFindRequest may cause a large result set to be created.")]
    [DoNotValidate]
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
            ForEntity<TEntity>()
                .SelectWith(builder => builder.Build(request => request.Key, "Id"));
        }
    }

    [Obsolete("Linq does not currently support positional queries. PagedFindRequest may cause a large result set to be created.")]
    [DoNotValidate]
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
            ForEntity<TEntity>()
                .SelectWith(builder => builder.Build(request => request.Key, "Guid"));
        }
    }

    #pragma warning restore 0618
}

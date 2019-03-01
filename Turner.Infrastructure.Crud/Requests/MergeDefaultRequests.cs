using System.Collections.Generic;
using AutoMapper;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    [MaybeValidate]
    public class MergeRequest<TEntity, TIn> : IMergeRequest<TEntity>
        where TEntity : class
    {
        public MergeRequest(List<TIn> items) { Items = items; }

        public List<TIn> Items { get; }
    }
    
    [MaybeValidate]
    public class MergeRequest<TEntity, TIn, TOut> : IMergeRequest<TEntity, TOut>
        where TEntity : class
    {
        public MergeRequest(List<TIn> items) { Items = items; }

        public List<TIn> Items { get; }
    }

    [MaybeValidate]
    public class MergeByIdRequest<TEntity, TIn> : MergeRequest<TEntity, TIn>
        where TEntity : class
    {
        public MergeByIdRequest(List<TIn> items) : base(items) { }
    }

    public class MergeByIdRequestProfile<TEntity, TIn>
        : CrudBulkRequestProfile<MergeByIdRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public MergeByIdRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Id");
        }
    }

    [MaybeValidate]
    public class MergeByIdRequest<TEntity, TIn, TOut> : MergeRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public MergeByIdRequest(List<TIn> items) : base(items) { }
    }

    public class MergeByIdRequestProfile<TEntity, TIn, TOut>
        : CrudBulkRequestProfile<MergeByIdRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public MergeByIdRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Id");
        }
    }

    [MaybeValidate]
    public class MergeByGuidRequest<TEntity, TIn> : MergeRequest<TEntity, TIn>
        where TEntity : class
    {
        public MergeByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class MergeByGuidRequestProfile<TEntity, TIn>
        : CrudBulkRequestProfile<MergeByGuidRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public MergeByGuidRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Guid");
        }
    }
    
    [MaybeValidate]
    public class MergeByGuidRequest<TEntity, TIn, TOut> : MergeRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public MergeByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class MergeByGuidRequestProfile<TEntity, TIn, TOut>
        : CrudBulkRequestProfile<MergeByGuidRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public MergeByGuidRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Guid");
        }
    }

    [MaybeValidate]
    public class MergeByNameRequest<TEntity, TIn> : MergeRequest<TEntity, TIn>
        where TEntity : class
    {
        public MergeByNameRequest(List<TIn> items) : base(items) { }
    }

    public class MergeByNameRequestProfile<TEntity, TIn>
        : CrudBulkRequestProfile<MergeByNameRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public MergeByNameRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Name");
        }
    }

    [MaybeValidate]
    public class MergeByNameRequest<TEntity, TIn, TOut> : MergeRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public MergeByNameRequest(List<TIn> items) : base(items) { }
    }

    public class MergeByNameRequestProfile<TEntity, TIn, TOut>
        : CrudBulkRequestProfile<MergeByNameRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public MergeByNameRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Name");
        }
    }
}

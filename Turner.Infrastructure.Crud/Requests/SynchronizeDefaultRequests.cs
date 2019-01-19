using System.Collections.Generic;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    [MaybeValidate]
    public class SynchronizeRequest<TEntity, TIn> : ISynchronizeRequest<TEntity>
        where TEntity : class
    {
        public SynchronizeRequest(List<TIn> items) { Items = items; }

        public List<TIn> Items { get; }
    }

    [MaybeValidate]
    public class SynchronizeByIdRequest<TEntity, TIn> : SynchronizeRequest<TEntity, TIn>
        where TEntity : class
    {
        public SynchronizeByIdRequest(List<TIn> items) : base(items) { }
    }

    public class SynchronizeByIdRequestProfile<TEntity, TIn>
        : CrudBulkRequestProfile<SynchronizeByIdRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public SynchronizeByIdRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Id");
        }
    }

    [MaybeValidate]
    public class SynchronizeByGuidRequest<TEntity, TIn> : SynchronizeRequest<TEntity, TIn>
        where TEntity : class
    {
        public SynchronizeByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class SynchronizeByGuidRequestProfile<TEntity, TIn>
        : CrudBulkRequestProfile<SynchronizeByGuidRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public SynchronizeByGuidRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Guid");
        }
    }

    [MaybeValidate]
    public class SynchronizeRequest<TEntity, TIn, TOut> : ISynchronizeRequest<TEntity, TOut>
        where TEntity : class
    {
        public SynchronizeRequest(List<TIn> items) { Items = items; }

        public List<TIn> Items { get; }
    }

    [MaybeValidate]
    public class SynchronizeByIdRequest<TEntity, TIn, TOut> : SynchronizeRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public SynchronizeByIdRequest(List<TIn> items) : base(items) { }
    }

    public class SynchronizeByIdRequestProfile<TEntity, TIn, TOut>
        : CrudBulkRequestProfile<SynchronizeByIdRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public SynchronizeByIdRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Id");
        }
    }

    [MaybeValidate]
    public class SynchronizeByGuidRequest<TEntity, TIn, TOut> : SynchronizeRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public SynchronizeByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class SynchronizeByGuidRequestProfile<TEntity, TIn, TOut>
        : CrudBulkRequestProfile<SynchronizeByGuidRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public SynchronizeByGuidRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Guid");
        }
    }
}

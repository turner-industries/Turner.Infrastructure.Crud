using System.Collections.Generic;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Requests
{
    [DoNotValidate]
    public class UpdateAllRequest<TEntity, TIn> : IUpdateAllRequest<TEntity>
        where TEntity : class
    {
        public UpdateAllRequest(List<TIn> items) { Items = items; }

        public List<TIn> Items { get; }
    }

    [DoNotValidate]
    public class UpdateAllByIdRequest<TEntity, TIn> : UpdateAllRequest<TEntity, TIn>
        where TEntity : class
    {
        public UpdateAllByIdRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByIdRequestProfile<TEntity, TIn>
        : CrudBulkRequestProfile<UpdateAllByIdRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public UpdateAllByIdRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Id");
        }
    }

    [DoNotValidate]
    public class UpdateAllByGuidRequest<TEntity, TIn> : UpdateAllRequest<TEntity, TIn>
        where TEntity : class
    {
        public UpdateAllByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByGuidRequestProfile<TEntity, TIn>
        : CrudBulkRequestProfile<UpdateAllByGuidRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public UpdateAllByGuidRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Guid");
        }
    }

    [DoNotValidate]
    public class UpdateAllRequest<TEntity, TIn, TOut> : IUpdateAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public UpdateAllRequest(List<TIn> items) { Items = items; }

        public List<TIn> Items { get; }
    }

    [DoNotValidate]
    public class UpdateAllByIdRequest<TEntity, TIn, TOut> : UpdateAllRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public UpdateAllByIdRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByIdRequestProfile<TEntity, TIn, TOut>
        : CrudBulkRequestProfile<UpdateAllByIdRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public UpdateAllByIdRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Id");
        }
    }

    [DoNotValidate]
    public class UpdateAllByGuidRequest<TEntity, TIn, TOut> : UpdateAllRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public UpdateAllByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByGuidRequestProfile<TEntity, TIn, TOut>
        : CrudBulkRequestProfile<UpdateAllByGuidRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public UpdateAllByGuidRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .WithKeys("Guid");
        }
    }
}

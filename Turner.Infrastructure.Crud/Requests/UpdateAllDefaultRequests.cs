using System.Collections.Generic;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    [MaybeValidate]
    public class UpdateAllRequest<TEntity, TIn> : IUpdateAllRequest<TEntity>
        where TEntity : class
    {
        public List<TIn> Items { get; set; }

        public UpdateAllRequest(List<TIn> items) { Items = items; }
    }

    [MaybeValidate]
    public class UpdateAllRequest<TEntity, TIn, TOut> : IUpdateAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public List<TIn> Items { get; set; }

        public UpdateAllRequest(List<TIn> items) { Items = items; }
    }
    
    [MaybeValidate]
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
                .UseKeys("Id");
        }
    }

    [MaybeValidate]
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
                .UseKeys("Id");
        }
    }

    [MaybeValidate]
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
                .UseKeys("Guid");
        }
    }
    
    [MaybeValidate]
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
                .UseKeys("Guid");
        }
    }

    [MaybeValidate]
    public class UpdateAllByNameRequest<TEntity, TIn> : UpdateAllRequest<TEntity, TIn>
            where TEntity : class
    {
        public UpdateAllByNameRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByNameRequestProfile<TEntity, TIn>
        : CrudBulkRequestProfile<UpdateAllByNameRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public UpdateAllByNameRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Name");
        }
    }

    [MaybeValidate]
    public class UpdateAllByNameRequest<TEntity, TIn, TOut> : UpdateAllRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public UpdateAllByNameRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByNameRequestProfile<TEntity, TIn, TOut>
        : CrudBulkRequestProfile<UpdateAllByNameRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public UpdateAllByNameRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .UseKeys("Name");
        }
    }
}

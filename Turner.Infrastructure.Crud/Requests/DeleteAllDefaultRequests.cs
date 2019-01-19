using System;
using System.Collections.Generic;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Requests
{
    [DoNotValidate, MaybeValidate]
    public class DeleteAllRequest<TEntity, TKey> : IDeleteAllRequest<TEntity>
        where TEntity : class
    {
        public DeleteAllRequest(List<TKey> keys) { Keys = keys; }

        public List<TKey> Keys { get; }
    }

    [DoNotValidate, MaybeValidate]
    public class DeleteAllRequest<TEntity, TKey, TOut> : IDeleteAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public DeleteAllRequest(List<TKey> keys) { Keys = keys; }

        public List<TKey> Keys { get; }
    }

    [DoNotValidate, MaybeValidate]
    public class DeleteAllByIdRequest<TEntity> : DeleteAllRequest<TEntity, int>
        where TEntity : class
    {
        public DeleteAllByIdRequest(List<int> ids) : base(ids) { }
    }

    public class DeleteAllByIdRequestProfile<TEntity>
        : CrudRequestProfile<DeleteAllByIdRequest<TEntity>>
        where TEntity : class
    {
        public DeleteAllByIdRequestProfile()
        {
            ForEntity<TEntity>()
                .FilterWith(builder => builder.FilterOn(request => request.Keys, "Id"));
        }
    }

    [DoNotValidate, MaybeValidate]
    public class DeleteAllByIdRequest<TEntity, TOut> : DeleteAllRequest<TEntity, int, TOut>
        where TEntity : class
    {
        public DeleteAllByIdRequest(List<int> ids) : base(ids) { }
    }

    public class DeleteAllByIdRequestProfile<TEntity, TOut>
        : CrudRequestProfile<DeleteAllByIdRequest<TEntity, TOut>>
        where TEntity : class
    {
        public DeleteAllByIdRequestProfile()
        {
            ForEntity<TEntity>()
                .FilterWith(builder => builder.FilterOn(request => request.Keys, "Id"));
        }
    }

    [DoNotValidate, MaybeValidate]
    public class DeleteAllByGuidRequest<TEntity> : DeleteAllRequest<TEntity, Guid>
        where TEntity : class
    {
        public DeleteAllByGuidRequest(List<Guid> guids) : base(guids) { }
    }

    public class DeleteAllByGuidRequestProfile<TEntity>
        : CrudRequestProfile<DeleteAllByGuidRequest<TEntity>>
        where TEntity : class
    {
        public DeleteAllByGuidRequestProfile()
        {
            ForEntity<TEntity>()
                .FilterWith(builder => builder.FilterOn(request => request.Keys, "Guid"));
        }
    }

    [DoNotValidate, MaybeValidate]
    public class DeleteAllByGuidRequest<TEntity, TOut> : DeleteAllRequest<TEntity, Guid, TOut>
        where TEntity : class
    {
        public DeleteAllByGuidRequest(List<Guid> guids) : base(guids) { }
    }

    public class DeleteAllByGuidRequestProfile<TEntity, TOut>
        : CrudRequestProfile<DeleteAllByGuidRequest<TEntity, TOut>>
        where TEntity : class
    {
        public DeleteAllByGuidRequestProfile()
        {
            ForEntity<TEntity>()
                .FilterWith(builder => builder.FilterOn(request => request.Keys, "Guid"));
        }
    }
}

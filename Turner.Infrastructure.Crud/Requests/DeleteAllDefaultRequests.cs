using System;
using System.Collections.Generic;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    [MaybeValidate]
    public class DeleteAllRequest<TEntity, TKey> : IDeleteAllRequest<TEntity>
        where TEntity : class
    {
        public List<TKey> Keys { get; set; } = new List<TKey>();

        public DeleteAllRequest() { }

        public DeleteAllRequest(List<TKey> keys) { Keys = keys; }
    }

    [MaybeValidate]
    public class DeleteAllRequest<TEntity, TKey, TOut> : IDeleteAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public DeleteAllRequest(List<TKey> keys) { Keys = keys; }

        public List<TKey> Keys { get; }
    }

    [MaybeValidate]
    public class DeleteAllByIdRequest<TEntity> : DeleteAllRequest<TEntity, int>
        where TEntity : class
    {
        public DeleteAllByIdRequest(List<int> ids) : base(ids) { }
    }

    public class DeleteAllByIdRequestProfile<TEntity>
        : RequestProfile<DeleteAllByIdRequest<TEntity>>
        where TEntity : class
    {
        public DeleteAllByIdRequestProfile()
        {
            ForEntity<TEntity>()
                .FilterOn(request => request.Keys, "Id");
        }
    }

    [MaybeValidate]
    public class DeleteAllByIdRequest<TEntity, TOut> : DeleteAllRequest<TEntity, int, TOut>
        where TEntity : class
    {
        public DeleteAllByIdRequest(List<int> ids) : base(ids) { }
    }

    public class DeleteAllByIdRequestProfile<TEntity, TOut>
        : RequestProfile<DeleteAllByIdRequest<TEntity, TOut>>
        where TEntity : class
    {
        public DeleteAllByIdRequestProfile()
        {
            ForEntity<TEntity>()
                .FilterOn(request => request.Keys, "Id");
        }
    }

    [MaybeValidate]
    public class DeleteAllByGuidRequest<TEntity> : DeleteAllRequest<TEntity, Guid>
        where TEntity : class
    {
        public DeleteAllByGuidRequest(List<Guid> guids) : base(guids) { }
    }

    public class DeleteAllByGuidRequestProfile<TEntity>
        : RequestProfile<DeleteAllByGuidRequest<TEntity>>
        where TEntity : class
    {
        public DeleteAllByGuidRequestProfile()
        {
            ForEntity<TEntity>()
                .FilterOn(request => request.Keys, "Guid");
        }
    }

    [MaybeValidate]
    public class DeleteAllByGuidRequest<TEntity, TOut> : DeleteAllRequest<TEntity, Guid, TOut>
        where TEntity : class
    {
        public DeleteAllByGuidRequest(List<Guid> guids) : base(guids) { }
    }

    public class DeleteAllByGuidRequestProfile<TEntity, TOut>
        : RequestProfile<DeleteAllByGuidRequest<TEntity, TOut>>
        where TEntity : class
    {
        public DeleteAllByGuidRequestProfile()
        {
            ForEntity<TEntity>()
                .FilterOn(request => request.Keys, "Guid");
        }
    }

    [MaybeValidate]
    public class DeleteAllByNameRequest<TEntity> : DeleteAllRequest<TEntity, string>
        where TEntity : class
    {
        public DeleteAllByNameRequest(List<string> names) : base(names) { }
    }

    public class DeleteAllByNameRequestProfile<TEntity>
        : RequestProfile<DeleteAllByNameRequest<TEntity>>
        where TEntity : class
    {
        public DeleteAllByNameRequestProfile()
        {
            ForEntity<TEntity>()
                .FilterOn(request => request.Keys, "Name");
        }
    }

    [MaybeValidate]
    public class DeleteAllByNameRequest<TEntity, TOut> : DeleteAllRequest<TEntity, string, TOut>
        where TEntity : class
    {
        public DeleteAllByNameRequest(List<string> names) : base(names) { }
    }

    public class DeleteAllByNameRequestProfile<TEntity, TOut>
        : RequestProfile<DeleteAllByNameRequest<TEntity, TOut>>
        where TEntity : class
    {
        public DeleteAllByNameRequestProfile()
        {
            ForEntity<TEntity>()
                .FilterOn(request => request.Keys, "Name");
        }
    }
}

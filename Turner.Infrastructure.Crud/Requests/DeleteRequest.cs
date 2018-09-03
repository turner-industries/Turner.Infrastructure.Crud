using System;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IDeleteRequest : ICrudRequest
    {
    }

    public interface IDeleteRequest<TEntity> : IDeleteRequest, IRequest
        where TEntity : class
    {       
    }

    public interface IDeleteRequest<TEntity, TOut> : IDeleteRequest, IRequest<TOut>
        where TEntity : class
    {
    }

    [DoNotValidate]
    public class DeleteRequest<TEntity, TKey> : IDeleteRequest<TEntity>
        where TEntity : class
    {
        public DeleteRequest(TKey key) { Key = key; }
        public TKey Key { get; }
    }

    public class DeleteRequestProfile<TEntity, TKey> 
        : CrudRequestProfile<CreateRequest<TEntity, TKey>>
        where TEntity : class
    {
        public DeleteRequestProfile() { }
    }

    [DoNotValidate]
    public class DeleteRequest<TEntity, TKey, TOut> : IDeleteRequest<TEntity, TOut>
        where TEntity : class
    {
        public DeleteRequest(TKey key) { Key = key; }
        public TKey Key { get; }
    }

    public class DeleteRequestProfile<TEntity, TKey, TOut>
        : CrudRequestProfile<CreateRequest<TEntity, TKey, TOut>>
        where TEntity : class
    {
        public DeleteRequestProfile() { }
    }

    [DoNotValidate]
    public class DeleteByIdRequest<TEntity> : DeleteRequest<TEntity, int>
        where TEntity : class
    {
        public DeleteByIdRequest(int id) : base(id) { }
    }

    public class DeleteByIdRequestProfile<TEntity>
        : CrudRequestProfile<DeleteByIdRequest<TEntity>>
        where TEntity : class
    {
        public DeleteByIdRequestProfile()
        {
            ForEntity<TEntity>()
                .SelectForDeleteWith(builder => builder.Build("Key", "Id"));
        }
    }

    [DoNotValidate]
    public class DeleteByIdRequest<TEntity, TOut> : DeleteRequest<TEntity, int, TOut>
        where TEntity : class
    {
        public DeleteByIdRequest(int id) : base(id) { }
    }

    public class DeleteByIdRequestProfile<TEntity, TOut>
        : CrudRequestProfile<DeleteByIdRequest<TEntity, TOut>>
        where TEntity : class
    {
        public DeleteByIdRequestProfile()
        {
            ForEntity<TEntity>()
                .SelectForDeleteWith(builder => builder.Build("Key", "Id"));
        }
    }

    [DoNotValidate]
    public class DeleteByGuidRequest<TEntity> : DeleteRequest<TEntity, Guid>
        where TEntity : class
    {
        public DeleteByGuidRequest(Guid guid) : base(guid) { }
    }

    public class DeleteByGuidRequestProfile<TEntity>
        : CrudRequestProfile<DeleteByGuidRequest<TEntity>>
        where TEntity : class
    {
        public DeleteByGuidRequestProfile()
        {
            ForEntity<TEntity>()
                .SelectForDeleteWith(builder => builder.Build("Key", "Guid"));
        }
    }

    [DoNotValidate]
    public class DeleteByGuidRequest<TEntity, TOut> : DeleteRequest<TEntity, Guid, TOut>
        where TEntity : class
    {
        public DeleteByGuidRequest(Guid guid) : base(guid) { }
    }

    public class DeleteByGuidRequestProfile<TEntity, TOut>
        : CrudRequestProfile<DeleteByGuidRequest<TEntity, TOut>>
        where TEntity : class
    {
        public DeleteByGuidRequestProfile()
        {
            ForEntity<TEntity>()
                .SelectForDeleteWith(builder => builder.Build("Key", "Guid"));
        }
    }
}

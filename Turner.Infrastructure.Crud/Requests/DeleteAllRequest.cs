using System;
using System.Collections.Generic;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;
// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IDeleteAllRequest : IBulkRequest
    {
    }

    public interface IDeleteAllRequest<TEntity> : IDeleteAllRequest, IRequest
        where TEntity : class
    {
    }

    public interface IDeleteAllRequest<TEntity, TOut> : IDeleteAllRequest, IRequest<DeleteAllResult<TOut>>
        where TEntity : class
    {
    }

    public class DeleteAllResult<TOut>
    {
        public List<TOut> Items { get; }

        public DeleteAllResult(List<TOut> items)
        {
            Items = items;
        }
    }

    [DoNotValidate]
    public class DeleteAllRequest<TEntity, TKey> : IDeleteAllRequest<TEntity>
        where TEntity : class
    {
        public DeleteAllRequest(List<TKey> keys) { Keys = keys; }

        public List<TKey> Keys { get; }
    }
    
    [DoNotValidate]
    public class DeleteAllRequest<TEntity, TKey, TOut> : IDeleteAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public DeleteAllRequest(List<TKey> keys) { Keys = keys; }

        public List<TKey> Keys { get; }
    }

    [DoNotValidate]
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
                .FilterWith(builder => builder.FilterOnCollection(request => request.Keys, "Id"));
        }
    }

    [DoNotValidate]
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
                .FilterWith(builder => builder.FilterOnCollection(request => request.Keys, "Id"));
        }
    }

    [DoNotValidate]
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
                .FilterWith(builder => builder.FilterOnCollection(request => request.Keys, "Guid"));
        }
    }

    [DoNotValidate]
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
                .FilterWith(builder => builder.FilterOnCollection(request => request.Keys, "Guid"));
        }
    }
}

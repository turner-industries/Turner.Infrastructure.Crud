using System;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;
// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IGetRequest : ICrudRequest
    {
    }

    public interface IGetRequest<TEntity, TOut> : IGetRequest, IRequest<TOut>
        where TEntity : class
    {
    }
    
    [DoNotValidate]
    public class GetRequest<TEntity, TKey, TOut> : IGetRequest<TEntity, TOut>
        where TEntity : class
    {
        public GetRequest(TKey key) { Key = key; }

        public TKey Key { get; }
    }
    
    [DoNotValidate]
    public class GetByIdRequest<TEntity, TOut> : GetRequest<TEntity, int, TOut>
        where TEntity : class
    {
        public GetByIdRequest(int id) : base(id) { }
    }

    public class GetByIdRequestProfile<TEntity, TOut>
        : CrudRequestProfile<GetByIdRequest<TEntity, TOut>>
        where TEntity : class
    {
        public GetByIdRequestProfile()
        {
            ForEntity<TEntity>()
                .SelectWith(builder => builder.Single(request => request.Key, "Id"));
        }
    }

    [DoNotValidate]
    public class GetByGuidRequest<TEntity, TOut> : GetRequest<TEntity, Guid, TOut>
        where TEntity : class
    {
        public GetByGuidRequest(Guid guid) : base(guid) { }
    }

    public class GetByGuidRequestProfile<TEntity, TOut>
        : CrudRequestProfile<GetByGuidRequest<TEntity, TOut>>
        where TEntity : class
    {
        public GetByGuidRequestProfile()
        {
            ForEntity<TEntity>()
                .SelectWith(builder => builder.Single(request => request.Key, "Guid"));
        }
    }
}

using System;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Requests
{
    [DoNotValidate, MaybeValidate]
    public class GetRequest<TEntity, TKey, TOut> : IGetRequest<TEntity, TOut>
        where TEntity : class
    {
        public GetRequest(TKey key) { Key = key; }

        public TKey Key { get; }
    }

    public class GetRequestProfile<TEntity, TKey, TOut>
        : CrudRequestProfile<GetRequest<TEntity, TKey, TOut>>
        where TEntity : class
    {
        public GetRequestProfile()
        {
            ForEntity<TEntity>().WithRequestKey(request => request.Key);
        }
    }

    [DoNotValidate, MaybeValidate]
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
            ForEntity<TEntity>().WithEntityKey("Id");
        }
    }

    [DoNotValidate, MaybeValidate]
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
            ForEntity<TEntity>().WithEntityKey("Guid");
        }
    }
}

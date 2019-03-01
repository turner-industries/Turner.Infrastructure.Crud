using System;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    [MaybeValidate]
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
            ForEntity<TEntity>().UseRequestKey(request => request.Key);
        }
    }

    [MaybeValidate]
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
            ForEntity<TEntity>().UseEntityKey("Id");
        }
    }

    [MaybeValidate]
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
            ForEntity<TEntity>().UseEntityKey("Guid");
        }
    }

    [MaybeValidate]
    public class GetByNameRequest<TEntity, TOut> : GetRequest<TEntity, string, TOut>
        where TEntity : class
    {
        public GetByNameRequest(string name) : base(name) { }
    }

    public class GetByNameRequestProfile<TEntity, TOut>
        : CrudRequestProfile<GetByNameRequest<TEntity, TOut>>
        where TEntity : class
    {
        public GetByNameRequestProfile()
        {
            ForEntity<TEntity>().UseEntityKey("Name");
        }
    }
}

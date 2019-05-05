using System;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    [MaybeValidate]
    public class DeleteRequest<TEntity, TKey> : IDeleteRequest<TEntity>
        where TEntity : class
    {
        public TKey Key { get; set; }

        public DeleteRequest() { }

        public DeleteRequest(TKey key) { Key = key; }
    }

    public class DeleteRequestProfile<TEntity, TKey>
        : CrudRequestProfile<DeleteRequest<TEntity, TKey>>
        where TEntity : class
    {
        public DeleteRequestProfile()
        {
            ForEntity<TEntity>().UseRequestKey(request => request.Key);
        }
    }

    [MaybeValidate]
    public class DeleteRequest<TEntity, TKey, TOut> : IDeleteRequest<TEntity, TOut>
        where TEntity : class
    {
        public TKey Key { get; set; }

        public DeleteRequest(TKey key) { Key = key; }
    }

    public class DeleteRequestProfile<TEntity, TKey, TOut>
        : CrudRequestProfile<DeleteRequest<TEntity, TKey, TOut>>
        where TEntity : class
    {
        public DeleteRequestProfile()
        {
            ForEntity<TEntity>().UseRequestKey(request => request.Key);
        }
    }

    [MaybeValidate]
    public class DeleteByIdRequest<TEntity>
        : DeleteRequest<TEntity, int>
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
            ForEntity<TEntity>().UseEntityKey("Id");
        }
    }

    [MaybeValidate]
    public class DeleteByIdRequest<TEntity, TOut>
        : DeleteRequest<TEntity, int, TOut>
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
            ForEntity<TEntity>().UseEntityKey("Id");
        }
    }

    [MaybeValidate]
    public class DeleteByGuidRequest<TEntity>
        : DeleteRequest<TEntity, Guid>
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
            ForEntity<TEntity>().UseEntityKey("Guid");
        }
    }

    [MaybeValidate]
    public class DeleteByGuidRequest<TEntity, TOut>
        : DeleteRequest<TEntity, Guid, TOut>
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
            ForEntity<TEntity>().UseEntityKey("Guid");
        }
    }

    [MaybeValidate]
    public class DeleteByNameRequest<TEntity>
        : DeleteRequest<TEntity, string>
        where TEntity : class
    {
        public DeleteByNameRequest(string name) : base(name) { }
    }

    public class DeleteByNameRequestProfile<TEntity>
        : CrudRequestProfile<DeleteByNameRequest<TEntity>>
        where TEntity : class
    {
        public DeleteByNameRequestProfile()
        {
            ForEntity<TEntity>().UseEntityKey("Name");
        }
    }

    [MaybeValidate]
    public class DeleteByNameRequest<TEntity, TOut>
        : DeleteRequest<TEntity, string, TOut>
        where TEntity : class
    {
        public DeleteByNameRequest(string name) : base(name) { }
    }

    public class DeleteByNameRequestProfile<TEntity, TOut>
        : CrudRequestProfile<DeleteByNameRequest<TEntity, TOut>>
        where TEntity : class
    {
        public DeleteByNameRequestProfile()
        {
            ForEntity<TEntity>().UseEntityKey("Name");
        }
    }
}

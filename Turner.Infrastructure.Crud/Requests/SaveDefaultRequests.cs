using AutoMapper;
using System;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    [MaybeValidate]
    public class SaveRequest<TEntity, TIn> : ISaveRequest<TEntity>
        where TEntity : class
    {
        public SaveRequest(TIn item) { Item = item; }

        public TIn Item { get; }
    }

    public class SaveRequestProfile<TEntity, TIn>
        : CrudRequestProfile<SaveRequest<TEntity, TIn>>
        where TEntity : class
    {
        public SaveRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateEntityWith(request => Mapper.Map<TEntity>(request.Item))
                .UpdateEntityWith((request, entity) => Mapper.Map(request.Item, entity));
        }
    }

    [MaybeValidate]
    public class SaveRequest<TEntity, TIn, TOut> : ISaveRequest<TEntity, TOut>
        where TEntity : class
    {
        public SaveRequest(TIn item) { Item = item; }

        public TIn Item { get; }
    }

    public class SaveRequestProfile<TEntity, TIn, TOut>
        : CrudRequestProfile<SaveRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public SaveRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateEntityWith(request => Mapper.Map<TEntity>(request.Item))
                .UpdateEntityWith((request, entity) => Mapper.Map(request.Item, entity));
        }
    }

    [MaybeValidate]
    public class SaveRequest<TEntity, TKey, TIn, TOut>
        : ISaveRequest<TEntity, TOut>
        where TEntity : class
    {
        public SaveRequest(TKey key, TIn item)
        {
            Key = key;
            Item = item;
        }

        public TKey Key { get; }

        public TIn Item { get; }
    }

    public class SaveRequestProfile<TEntity, TKey, TIn, TOut>
        : CrudRequestProfile<SaveRequest<TEntity, TKey, TIn, TOut>>
        where TEntity : class
    {
        public SaveRequestProfile()
        {
            ForEntity<TEntity>()
                .WithRequestKey(request => request.Key)
                .CreateEntityWith(request => Mapper.Map<TEntity>(request.Item))
                .UpdateEntityWith((request, entity) => Mapper.Map(request.Item, entity));
        }
    }

    [MaybeValidate]
    public class SaveByIdRequest<TEntity, TIn, TOut> : SaveRequest<TEntity, int, TIn, TOut>
        where TEntity : class
    {
        public SaveByIdRequest(int id, TIn data) : base(id, data) { }
    }

    public class SaveByIdRequestProfile<TEntity, TIn, TOut>
        : CrudRequestProfile<SaveByIdRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public SaveByIdRequestProfile()
        {
            ForEntity<TEntity>().WithEntityKey("Id");
        }
    }

    [MaybeValidate]
    public class SaveByGuidRequest<TEntity, TIn, TOut> : SaveRequest<TEntity, Guid, TIn, TOut>
        where TEntity : class
    {
        public SaveByGuidRequest(Guid guid, TIn data) : base(guid, data) { }
    }

    public class SaveByGuidRequestProfile<TEntity, TIn, TOut>
        : CrudRequestProfile<SaveByGuidRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public SaveByGuidRequestProfile()
        {
            ForEntity<TEntity>().WithEntityKey("Guid");
        }
    }

    [MaybeValidate]
    public class SaveByNameRequest<TEntity, TIn, TOut> : SaveRequest<TEntity, string, TIn, TOut>
        where TEntity : class
    {
        public SaveByNameRequest(string name, TIn data) : base(name, data) { }
    }

    public class SaveByNameRequestProfile<TEntity, TIn, TOut>
        : CrudRequestProfile<SaveByNameRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public SaveByNameRequestProfile()
        {
            ForEntity<TEntity>().WithEntityKey("Name");
        }
    }
}

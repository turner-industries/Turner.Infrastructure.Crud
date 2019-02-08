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
        public SaveRequest(TIn data) { Data = data; }

        public TIn Data { get; }
    }

    public class SaveRequestProfile<TEntity, TIn>
        : CrudRequestProfile<SaveRequest<TEntity, TIn>>
        where TEntity : class
    {
        public SaveRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateEntityWith((SaveRequest<TEntity, TIn> request) => Mapper.Map<TEntity>(request.Data))
                .UpdateEntityWith((SaveRequest<TEntity, TIn> request, TEntity entity) => Mapper.Map(request.Data, entity));
        }
    }

    [MaybeValidate]
    public class SaveRequest<TEntity, TIn, TOut> : ISaveRequest<TEntity, TOut>
        where TEntity : class
    {
        public SaveRequest(TIn data) { Data = data; }

        public TIn Data { get; }
    }

    public class SaveRequestProfile<TEntity, TIn, TOut>
        : CrudRequestProfile<SaveRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public SaveRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateEntityWith((SaveRequest<TEntity, TIn, TOut> request) => Mapper.Map<TEntity>(request.Data))
                .UpdateEntityWith((SaveRequest<TEntity, TIn, TOut> request, TEntity entity) => Mapper.Map(request.Data, entity));
        }
    }

    [MaybeValidate]
    public class SaveRequest<TEntity, TKey, TIn, TOut>
        : ISaveRequest<TEntity, TOut>
        where TEntity : class
    {
        public TKey Key { get; }

        public TIn Data { get; }

        public SaveRequest(TKey key, TIn data)
        {
            Key = key;
            Data = data;
        }
    }

    public class SaveRequestProfile<TEntity, TKey, TIn, TOut>
        : CrudRequestProfile<SaveRequest<TEntity, TKey, TIn, TOut>>
        where TEntity : class
    {
        public SaveRequestProfile()
        {
            ForEntity<TEntity>()
                .WithRequestKey(request => request.Key)
                .CreateEntityWith(request => Mapper.Map<TEntity>(request.Data))
                .UpdateEntityWith((request, entity) => Mapper.Map(request.Data, entity));
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
}

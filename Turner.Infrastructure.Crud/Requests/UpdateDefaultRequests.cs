using AutoMapper;
using System;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    [MaybeValidate]
    public class UpdateRequest<TEntity, TIn> : IUpdateRequest<TEntity>
        where TEntity : class
    {
        public UpdateRequest(TIn data) { Data = data; }

        public TIn Data { get; }
    }

    public class UpdateRequestProfile<TEntity, TIn>
        : CrudRequestProfile<UpdateRequest<TEntity, TIn>>
        where TEntity : class
    {
        public UpdateRequestProfile()
        {
            ForEntity<TEntity>()
                .UpdateWith((UpdateRequest<TEntity, TIn> request, TEntity entity) => Mapper.Map(request.Data, entity));
        }
    }

    [MaybeValidate]
    public class UpdateRequest<TEntity, TIn, TOut> : IUpdateRequest<TEntity, TOut>
        where TEntity : class
    {
        public UpdateRequest(TIn data) { Data = data; }

        public TIn Data { get; }
    }

    public class UpdateRequestProfile<TEntity, TIn, TOut>
        : CrudRequestProfile<UpdateRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public UpdateRequestProfile()
        {
            ForEntity<TEntity>()
                .UpdateWith((UpdateRequest<TEntity, TIn, TOut> request, TEntity entity) => Mapper.Map(request.Data, entity));
        }
    }

    [MaybeValidate]
    public class UpdateRequest<TEntity, TKey, TIn, TOut>
        : IUpdateRequest<TEntity, TOut>
        where TEntity : class
    {
        public TKey Key { get; }

        public TIn Data { get; }

        public UpdateRequest(TKey key, TIn data)
        {
            Key = key;
            Data = data;
        }
    }

    public class UpdateRequestProfile<TEntity, TKey, TIn, TOut>
        : CrudRequestProfile<UpdateRequest<TEntity, TKey, TIn, TOut>>
        where TEntity : class
    {
        public UpdateRequestProfile()
        {
            ForEntity<TEntity>()
                .UpdateWith((UpdateRequest<TEntity, TKey, TIn, TOut> request, TEntity entity) => Mapper.Map(request.Data, entity));
        }
    }

    [MaybeValidate]
    public class UpdateByIdRequest<TEntity, TIn, TOut> : UpdateRequest<TEntity, int, TIn, TOut>
        where TEntity : class
    {
        public UpdateByIdRequest(int id, TIn data) : base(id, data) { }
    }

    public class UpdateByIdRequestProfile<TEntity, TIn, TOut>
        : CrudRequestProfile<UpdateByIdRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public UpdateByIdRequestProfile()
        {
            ForEntity<TEntity>()
                .SelectWith(builder => builder.Single(request => request.Key, "Id"));
        }
    }

    [MaybeValidate]
    public class UpdateByGuidRequest<TEntity, TIn, TOut> : UpdateRequest<TEntity, Guid, TIn, TOut>
        where TEntity : class
    {
        public UpdateByGuidRequest(Guid guid, TIn data) : base(guid, data) { }
    }

    public class UpdateByGuidRequestProfile<TEntity, TIn, TOut>
        : CrudRequestProfile<UpdateByGuidRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public UpdateByGuidRequestProfile()
        {
            ForEntity<TEntity>()
                .SelectWith(builder => builder.Single(request => request.Key, "Guid"));
        }
    }
}

using AutoMapper;
using System;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IUpdateRequest : ICrudRequest
    {
    }

    public interface IUpdateRequest<TEntity> : IUpdateRequest, IRequest
        where TEntity : class
    {
    }

    public interface IUpdateRequest<TEntity, TOut> : IUpdateRequest, IRequest<TOut>
        where TEntity : class
    {
    }
    
    [DoNotValidate]
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
                .UpdateWith((request, entity) => Mapper.Map(request.Data, entity));
        }
    }

    [DoNotValidate]
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
                .UpdateWith((request, entity) => Mapper.Map(request.Data, entity));
        }
    }

    [DoNotValidate]
    public class UpdateRequest<TEntity, TKey, TIn, TOut> 
        : IUpdateRequest<TEntity, TOut>
        where TEntity : class
    {
        public UpdateRequest(TKey key, TIn data)
        {
            Key = key;
            Data = data;
        }

        public TKey Key { get; }
        public TIn Data { get; }
    }

    public class UpdateRequestProfile<TEntity, TKey, TIn, TOut>
        : CrudRequestProfile<UpdateRequest<TEntity, TKey, TIn, TOut>>
        where TEntity : class
    {
        public UpdateRequestProfile()
        {
            ForEntity<TEntity>()
                .UpdateWith((request, entity) => Mapper.Map(request.Data, entity));
        }
    }
    
    [DoNotValidate]
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
                .SelectForUpdateWith(builder => builder.Build(request => request.Key, "Id"));
        }
    }

    [DoNotValidate]
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
                .SelectForUpdateWith(builder => builder.Build(request => request.Key, "Guid"));
        }
    }
}

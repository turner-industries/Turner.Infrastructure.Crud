using AutoMapper;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface ISaveRequest : ICrudRequest
    {
    }

    public interface ISaveRequest<TEntity> : ISaveRequest, IRequest
        where TEntity : class
    {
    }

    public interface ISaveRequest<TEntity, TOut> : ISaveRequest, IRequest<TOut>
        where TEntity : class
    {
    }

    [DoNotValidate]
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
                .CreateWith(request => Mapper.Map<TEntity>(request.Data))
                .UpdateWith((request, entity) => Mapper.Map(request.Data, entity));
        }
    }

    [DoNotValidate]
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
                .CreateWith(request => Mapper.Map<TEntity>(request.Data))
                .UpdateWith((request, entity) => Mapper.Map(request.Data, entity));
        }
    }

    [DoNotValidate]
    public class SaveRequest<TEntity, TKey, TIn, TOut>
        : ISaveRequest<TEntity, TOut>
        where TEntity : class
    {
        public SaveRequest(TKey key, TIn data)
        {
            Key = key;
            Data = data;
        }

        public TKey Key { get; }
        public TIn Data { get; }
    }

    public class SaveRequestProfile<TEntity, TKey, TIn, TOut>
        : CrudRequestProfile<SaveRequest<TEntity, TKey, TIn, TOut>>
        where TEntity : class
    {
        public SaveRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateWith(request => Mapper.Map<TEntity>(request.Data))
                .UpdateWith((request, entity) => Mapper.Map(request.Data, entity));
        }
    }

    [DoNotValidate]
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
            ForEntity<TEntity>()
                .SelectForUpdateWith(builder => builder.Build("Key", "Id"));
        }
    }
}

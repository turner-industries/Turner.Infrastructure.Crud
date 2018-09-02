using AutoMapper;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface ICreateRequest : ICrudRequest
    {
    }

    public interface ICreateRequest<TEntity> : ICreateRequest, IRequest
        where TEntity : class
    {       
    }

    public interface ICreateRequest<TEntity, TOut> : ICreateRequest, IRequest<TOut>
        where TEntity : class
    {
    }

    [DoNotValidate]
    public class CreateRequest<TEntity, TIn> : ICreateRequest<TEntity>
        where TEntity : class
    {
        public CreateRequest(TIn data) { Data = data; }
        public TIn Data { get; }
    }

    public class CreateRequestProfile<TEntity, TIn> 
        : CrudRequestProfile<CreateRequest<TEntity, TIn>>
        where TEntity : class
    {
        public CreateRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateWith(x => Mapper.Map<TEntity>(x.Data));
        }
    }

    [DoNotValidate]
    public class CreateRequest<TEntity, TIn, TOut> : ICreateRequest<TEntity, TOut>
        where TEntity : class
    {
        public CreateRequest(TIn data) { Data = data; }
        public TIn Data { get; }
    }

    public class CreateRequestProfile<TEntity, TIn, TOut>
        : CrudRequestProfile<CreateRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public CreateRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateWith(x => Mapper.Map<TEntity>(x.Data));
        }
    }
}

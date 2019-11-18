using Turner.Infrastructure.Mediator;

// ReSharper disable UnusedTypeParameter

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
}

using Turner.Infrastructure.Mediator;

// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IDeleteRequest : ICrudRequest
    {
    }

    public interface IDeleteRequest<TEntity> : IDeleteRequest, IRequest
        where TEntity : class
    {       
    }

    public interface IDeleteRequest<TEntity, TOut> : IDeleteRequest, IRequest<TOut>
        where TEntity : class
    {
    }
}

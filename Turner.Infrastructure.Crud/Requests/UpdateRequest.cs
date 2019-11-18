using Turner.Infrastructure.Mediator;

// ReSharper disable UnusedTypeParameter

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
}

using Turner.Infrastructure.Mediator;

// ReSharper disable UnusedTypeParameter

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
}

using Turner.Infrastructure.Mediator;
// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IGetRequest : ICrudRequest
    {
    }

    public interface IGetRequest<TEntity, TOut> : IGetRequest, IRequest<TOut>
        where TEntity : class
    {
    }
}

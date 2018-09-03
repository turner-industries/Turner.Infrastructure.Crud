using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IGetRequest : ICrudRequest
    {
    }

    public interface IGetRequest<TEntity, TOut> : IGetRequest, IRequest<TOut>
        where TEntity : class
    {
    }
    
    [DoNotValidate]
    public class GetRequest<TEntity, TKey, TOut> : IGetRequest<TEntity, TOut>
        where TEntity : class
    {
        public GetRequest(TKey key) { Key = key; }
        public TKey Key { get; }
    }

    public class GetRequestProfile<TEntity, TKey, TOut>
        : CrudRequestProfile<GetRequest<TEntity, TKey, TOut>>
        where TEntity : class
    {
        public GetRequestProfile()
        {
        }
    }
}

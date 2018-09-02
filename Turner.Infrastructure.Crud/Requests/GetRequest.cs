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
    public class GetRequest<TEntity, TIn, TOut> : IGetRequest<TEntity, TOut>
        where TEntity : class
    {
        public GetRequest(TIn data) { Data = data; }
        public TIn Data { get; }
    }

    public class GetRequestProfile<TEntity, TIn, TOut>
        : CrudRequestProfile<GetRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public GetRequestProfile()
        {
        }
    }
}

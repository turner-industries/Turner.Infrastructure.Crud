using System.Collections.Generic;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IGetAllRequest : ICrudRequest
    {
    }

    public interface IGetAllRequest<TEntity, TOut> : IGetAllRequest, IRequest<List<TOut>>
        where TEntity : class
    {
    }

    [DoNotValidate]
    public class GetAllRequest<TEntity, TOut> : IGetAllRequest<TEntity, TOut>
        where TEntity : class
    {
    }
}

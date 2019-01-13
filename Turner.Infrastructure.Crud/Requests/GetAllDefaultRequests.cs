using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Requests
{
    [DoNotValidate]
    public class GetAllRequest<TEntity, TOut> : IGetAllRequest<TEntity, TOut>
        where TEntity : class
    {
    }
}

using Turner.Infrastructure.Crud.Validation;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Requests
{
    [DoNotValidate, MaybeValidate]
    public class GetAllRequest<TEntity, TOut> : IGetAllRequest<TEntity, TOut>
        where TEntity : class
    {
    }
}

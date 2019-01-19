using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    [MaybeValidate]
    public class GetAllRequest<TEntity, TOut> : IGetAllRequest<TEntity, TOut>
        where TEntity : class
    {
    }
}

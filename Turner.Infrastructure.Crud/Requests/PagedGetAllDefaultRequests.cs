using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    [MaybeValidate]
    public class PagedGetAllRequest<TEntity, TOut> : IPagedGetAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}

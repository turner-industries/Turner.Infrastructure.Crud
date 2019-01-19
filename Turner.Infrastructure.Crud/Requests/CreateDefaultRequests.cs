using AutoMapper;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    [MaybeValidate]
    public class CreateRequest<TEntity, TIn> 
        : ICreateRequest<TEntity>
        where TEntity : class
    {
        public CreateRequest(TIn item) { Item = item; }

        public TIn Item { get; }
    }

    public class CreateRequestProfile<TEntity, TIn>
        : CrudRequestProfile<CreateRequest<TEntity, TIn>>
        where TEntity : class
    {
        public CreateRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateWith(request => Mapper.Map<TEntity>(request.Item));
        }
    }

    [MaybeValidate]
    public class CreateRequest<TEntity, TIn, TOut> 
        : ICreateRequest<TEntity, TOut>
        where TEntity : class
    {
        public CreateRequest(TIn item) { Item = item; }

        public TIn Item { get; }
    }

    public class CreateRequestProfile<TEntity, TIn, TOut>
        : CrudRequestProfile<CreateRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public CreateRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateWith(request => Mapper.Map<TEntity>(request.Item));
        }
    }
}

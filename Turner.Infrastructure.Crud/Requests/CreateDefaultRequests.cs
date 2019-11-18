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
        public TIn Item { get; set; }

        public CreateRequest() { }

        public CreateRequest(TIn item) { Item = item; }
    }

    public class CreateRequestProfile<TEntity, TIn>
        : RequestProfile<CreateRequest<TEntity, TIn>>
        where TEntity : class
    {
        public CreateRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateEntityWith(request => Mapper.Map<TEntity>(request.Item));
        }
    }

    [MaybeValidate]
    public class CreateRequest<TEntity, TIn, TOut> 
        : ICreateRequest<TEntity, TOut>
        where TEntity : class
    {
        public TIn Item { get; set; }

        public CreateRequest() { }

        public CreateRequest(TIn item) { Item = item; }
    }

    public class CreateRequestProfile<TEntity, TIn, TOut>
        : RequestProfile<CreateRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public CreateRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateEntityWith(request => Mapper.Map<TEntity>(request.Item));
        }
    }
}

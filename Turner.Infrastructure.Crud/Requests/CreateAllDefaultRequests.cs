using AutoMapper;
using System.Collections.Generic;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.Requests
{
    [MaybeValidate]
    public class CreateAllRequest<TEntity, TIn> : ICreateAllRequest<TEntity>
        where TEntity : class
    {
        public CreateAllRequest(List<TIn> items) { Items = items; }

        public List<TIn> Items { get; }
    }

    public class CreateAllRequestProfile<TEntity, TIn>
        : CrudBulkRequestProfile<CreateAllRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public CreateAllRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .CreateEntityWith(item => Mapper.Map<TEntity>(item));
        }
    }

    [MaybeValidate]
    public class CreateAllRequest<TEntity, TIn, TOut> : ICreateAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public CreateAllRequest(List<TIn> items) { Items = items; }

        public List<TIn> Items { get; }
    }

    public class CreateAllRequestProfile<TEntity, TIn, TOut>
        : CrudBulkRequestProfile<CreateAllRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public CreateAllRequestProfile()
            : base(request => request.Items)
        {
            ForEntity<TEntity>()
                .CreateEntityWith(item => Mapper.Map<TEntity>(item));
        }
    }
}

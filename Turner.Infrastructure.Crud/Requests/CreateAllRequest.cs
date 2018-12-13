using AutoMapper;
using System.Collections.Generic;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;
// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
    public interface ICreateAllRequest : IBulkRequest
    {
    }

    public interface ICreateAllRequest<TEntity> : ICreateAllRequest, IRequest
        where TEntity : class
    {
    }

    public interface ICreateAllRequest<TEntity, TOut> : ICreateAllRequest, IRequest<CreateAllResult<TOut>>
        where TEntity : class
    {
    }

    public class CreateAllResult<TOut>
    {
        public List<TOut> Items { get; }

        public CreateAllResult(List<TOut> items)
        {
            Items = items;
        }
    }

    [DoNotValidate]
    public class CreateAllRequest<TEntity, TIn> : ICreateAllRequest<TEntity>
        where TEntity : class
    {
        public CreateAllRequest(List<TIn> items) { Items = items; }

        public List<TIn> Items { get; }
    }

    public class CreateAllRequestProfile<TEntity, TIn>
        : CrudRequestProfile<CreateAllRequest<TEntity, TIn>>
        where TEntity : class
    {
        public CreateAllRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateWith(request => Mapper.Map<TEntity[]>(request.Items));
        }
    }

    [DoNotValidate]
    public class CreateAllRequest<TEntity, TIn, TOut> : ICreateAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public CreateAllRequest(List<TIn> items) { Items = items; }

        public List<TIn> Items { get; }
    }

    public class CreateAllRequestProfile<TEntity, TIn, TOut>
        : CrudRequestProfile<CreateAllRequest<TEntity, TIn, TOut>>
        where TEntity : class
    {
        public CreateAllRequestProfile()
        {
            ForEntity<TEntity>()
                .CreateWith(request => Mapper.Map<TEntity[]>(request.Items));
        }
    }
}

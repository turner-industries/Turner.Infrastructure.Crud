using System.Collections.Generic;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;
// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IDeleteAllRequest : IBulkRequest
    {
    }

    public interface IDeleteAllRequest<TEntity> : IDeleteAllRequest, IRequest
        where TEntity : class
    {
    }

    public interface IDeleteAllRequest<TEntity, TOut> : IDeleteAllRequest, IRequest<DeleteAllResult<TOut>>
        where TEntity : class
    {
    }

    public class DeleteAllResult<TOut>
    {
        public List<TOut> Items { get; }

        public DeleteAllResult(List<TOut> items)
        {
            Items = items;
        }
    }

    // TODO: DeleteAllRequest ??

    [DoNotValidate]
    public class DeleteAllByIdRequest<TEntity> : IDeleteAllRequest<TEntity>
        where TEntity : class
    {
        public DeleteAllByIdRequest(List<int> ids) { Ids = ids; }

        public List<int> Ids { get; set; }
    }

    public class DeleteAllByIdRequestProfile<TEntity>
        : CrudRequestProfile<DeleteAllByIdRequest<TEntity>>
        where TEntity : class
    {
        public DeleteAllByIdRequestProfile()
        {
            // TODO: Either "filter by list" filters OR custom filter with an expression builder
            //ForEntity<TEntity>()
            //    .FilterWith(builder => builder.FilterOn((request, entity) => request.Ids.Contains("Id")));
        }
    }

    [DoNotValidate]
    public class DeleteAllByIdRequest<TEntity, TOut> : IDeleteAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public DeleteAllByIdRequest(List<int> ids) { Ids = ids; }

        public List<int> Ids { get; set; }
    }

    public class DeleteAllByIdRequestProfile<TEntity, TOut>
        : CrudRequestProfile<DeleteAllByIdRequest<TEntity, TOut>>
        where TEntity : class
    {
        public DeleteAllByIdRequestProfile()
        {
            //ForEntity<TEntity>()
            //    .FilterWith(builder => builder.FilterOn((request, entity) => request.Ids.Contains("Id")));
        }
    }
}

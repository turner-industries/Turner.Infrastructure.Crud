using System.Collections.Generic;
using System.Linq;
using Turner.Infrastructure.Mediator;
// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IDeleteAllRequest : ICrudRequest
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

    public class DeleteAllResult<TOut> : IResultCollection<TOut>
    {
        public List<TOut> Items { get; set; }

        public DeleteAllResult(IEnumerable<TOut> items)
        {
            Items = items.ToList();
        }
    }
}

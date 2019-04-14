using System.Collections.Generic;
using System.Linq;
using Turner.Infrastructure.Mediator;
// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IMergeRequest : IBulkRequest
    {
    }

    public interface IMergeRequest<TEntity> : IMergeRequest, IRequest
        where TEntity : class
    {
    }

    public interface IMergeRequest<TEntity, TOut> : IMergeRequest, IRequest<MergeResult<TOut>>
        where TEntity : class
    {
    }

    public class MergeResult<TOut>
    {
        public List<TOut> Items { get; }

        public MergeResult(IEnumerable<TOut> items)
        {
            Items = items.ToList();
        }
    }
}

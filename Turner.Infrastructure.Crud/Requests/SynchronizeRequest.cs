using System.Collections.Generic;
using System.Linq;
using Turner.Infrastructure.Mediator;

// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
    public interface ISynchronizeRequest : IBulkRequest
    {
    }

    public interface ISynchronizeRequest<TEntity> : ISynchronizeRequest, IRequest
        where TEntity : class
    {
    }

    public interface ISynchronizeRequest<TEntity, TOut> : ISynchronizeRequest, IRequest<SynchronizeResult<TOut>>
        where TEntity : class
    {
    }

    public class SynchronizeResult<TOut> : IResultCollection<TOut>
    {
        public List<TOut> Items { get; set; }

        public SynchronizeResult(IEnumerable<TOut> items)
        {
            Items = items.ToList();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Turner.Infrastructure.Mediator;

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

    public class CreateAllResult<TOut> : IResultCollection<TOut>
    {
        public List<TOut> Items { get; set; }

        public CreateAllResult(IEnumerable<TOut> items)
        {
            Items = items.ToList();
        }
    }
}

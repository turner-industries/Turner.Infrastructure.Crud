using System.Collections.Generic;
using System.Linq;
using Turner.Infrastructure.Mediator;
// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IGetAllRequest : ICrudRequest
    {
    }

    public interface IGetAllRequest<TEntity, TOut> : IGetAllRequest, IRequest<GetAllResult<TOut>>
        where TEntity : class
    {
    }
    
    public class GetAllResult<TOut> : IResultCollection<TOut>
    {
        public List<TOut> Items { get; set; }

        public GetAllResult(IEnumerable<TOut> items)
        {
            Items = items.ToList();
        }
    }
}

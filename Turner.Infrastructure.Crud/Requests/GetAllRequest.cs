using System.Collections.Generic;
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
    
    public class GetAllResult<TOut>
    {
        public List<TOut> Items { get; }

        public GetAllResult(List<TOut> items)
        {
            Items = items;
        }
    }
}

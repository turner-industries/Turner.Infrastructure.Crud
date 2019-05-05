using System.Collections.Generic;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IResultCollection<TOut>
    {
        List<TOut> Items { get; set; }
    }
}

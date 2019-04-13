using System.Collections.Generic;

namespace Turner.Infrastructure.Crud.Context
{
    internal interface IAsyncEnumerableAccessor<out T>
    {
        IAsyncEnumerable<T> AsyncEnumerable { get; }
    }
}

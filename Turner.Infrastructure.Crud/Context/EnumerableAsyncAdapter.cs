using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Context
{
    public class EnumerableAsyncAdapter<TResult> 
        : EnumerableQuery<TResult>, IAsyncEnumerable<TResult>
    {
        public EnumerableAsyncAdapter(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<TResult> GetEnumerator()
        {
            return new AsyncEnumerator(this.AsEnumerable().GetEnumerator());
        }

        private class AsyncEnumerator : IAsyncEnumerator<TResult>
        {
            private readonly IEnumerator<TResult> _inner;

            public AsyncEnumerator(IEnumerator<TResult> inner)
            {
                _inner = inner;
            }

            public void Dispose()
            {
                _inner.Dispose();
            }

            public TResult Current => _inner.Current;

            public Task<bool> MoveNext(CancellationToken cancellationToken)
                => Task.FromResult(_inner.MoveNext());
        }
    }
}

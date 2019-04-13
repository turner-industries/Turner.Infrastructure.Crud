using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Context
{
    public interface IEntityQueryProvider : IQueryProvider
    {
        IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression);

        Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken);
    }
}

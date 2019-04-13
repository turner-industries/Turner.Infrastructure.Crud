using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.Tests.ContextTests
{
    public class InMemoryQueryProvider : IEntityQueryProvider
    {
        private static readonly MethodInfo _createQuery
            = typeof(InMemoryQueryProvider)
                .GetRuntimeMethods()
                .Single(x => x.Name == "CreateQuery" && x.IsGenericMethod);
        
        private readonly IQueryProvider _queryProvider;

        public InMemoryQueryProvider(IQueryProvider queryProvider)
        {
            _queryProvider = queryProvider;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return (IQueryable)_createQuery
                .MakeGenericMethod(expression.Type.GetSequenceType())
                .Invoke(this, new object[] { expression });
        }

        public IQueryable<TEntity> CreateQuery<TEntity>(Expression expression)
            => new EntityQueryable<TEntity>(this, expression);

        public object Execute(Expression expression)
            => _queryProvider.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
            => _queryProvider.Execute<TResult>(expression);

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            => new AsyncEnumerable<TResult>(expression);

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
            => Task.FromResult(Execute<TResult>(expression));
    }
}

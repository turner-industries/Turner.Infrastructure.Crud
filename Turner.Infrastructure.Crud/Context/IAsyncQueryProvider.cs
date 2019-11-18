using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Context
{
    public interface IAsyncQueryProvider : IQueryProvider
    {
        IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression);

        Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken);
    }
    
    public static class AsyncQueryProviderExtensions
    {
        public static IAsyncQueryProvider AsAsyncQueryProvider(this IQueryProvider queryProvider)
        {
            if (queryProvider is null)
                throw new ArgumentNullException(nameof(queryProvider));

            if (queryProvider is IAsyncQueryProvider asyncQueryProvider)
                return asyncQueryProvider;

            return new AsyncAdaptedQueryProvider(queryProvider);
        }

        public class AsyncAdaptedQueryProvider : IAsyncQueryProvider
        {
            private static readonly MethodInfo CreateQueryMethod
                = typeof(AsyncAdaptedQueryProvider)
                    .GetRuntimeMethods()
                    .Single(x => x.Name == "CreateQuery" && x.IsGenericMethod);
            
            private readonly IQueryProvider _queryProvider;

            public AsyncAdaptedQueryProvider(IQueryProvider queryProvider)
            {
                _queryProvider = queryProvider;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return (IQueryable)CreateQueryMethod
                    .MakeGenericMethod(expression.Type.GetSequenceType())
                    .Invoke(this, new object[] { expression });
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
                => new EntityQueryable<TElement>(this, expression);

            public object Execute(Expression expression)
                => _queryProvider.Execute(expression);

            public TResult Execute<TResult>(Expression expression)
                => _queryProvider.Execute<TResult>(expression);

            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
                => new EnumerableAsyncAdapter<TResult>(expression);

            public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
                => Task.FromResult(Execute<TResult>(expression));
        }
    }
}

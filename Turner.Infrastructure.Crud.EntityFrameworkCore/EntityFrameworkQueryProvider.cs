using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.EntityFrameworkCore
{
    public class EntityFrameworkQueryProvider : IEntityQueryProvider
    {
        private static readonly MethodInfo _createQuery
            = typeof(EntityFrameworkQueryProvider)
                .GetRuntimeMethods()
                .Single(x => x.Name == "CreateQuery" && x.IsGenericMethod);

        private readonly IAsyncQueryProvider _queryProvider;
        
        internal EntityFrameworkQueryProvider(IQueryProvider queryProvider)
        {
            if (!(queryProvider is IAsyncQueryProvider asyncQueryProvider))
                throw new ArgumentException(nameof(queryProvider));

            _queryProvider = asyncQueryProvider;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return (IQueryable)_createQuery
                .MakeGenericMethod(expression.Type.GetSequenceType())
                .Invoke(this, new object[] { expression });
        }

        public IQueryable<TEntity> CreateQuery<TEntity>(Expression expression)
            => new Context.EntityQueryable<TEntity>(this, expression);

        public object Execute(Expression expression)
            => _queryProvider.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
            =>  _queryProvider.Execute<TResult>(expression);

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
            => _queryProvider.ExecuteAsync<TResult>(expression);

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
            => _queryProvider.ExecuteAsync<TResult>(expression, token);
    }
}

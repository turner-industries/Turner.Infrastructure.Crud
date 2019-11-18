using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud.Context
{
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    public interface IEntityQueryable<out TEntity> 
        : IOrderedQueryable<TEntity>, IAsyncEnumerable<TEntity>
    {
    }

    public class EntityQueryable<TEntity> : IEntityQueryable<TEntity>
    {
        protected readonly IAsyncQueryProvider QueryProvider;
        
        public EntityQueryable(IQueryProvider queryProvider, Expression expression)
        {
            QueryProvider = queryProvider.AsAsyncQueryProvider() 
                ?? throw new ArgumentNullException(nameof(queryProvider));

            Expression = expression 
                ?? throw new ArgumentNullException(nameof(expression));
        }

        public virtual IQueryProvider Provider => QueryProvider;

        public virtual Type ElementType => typeof(TEntity);

        public virtual Expression Expression { get; }

        public virtual IEnumerator<TEntity> GetEnumerator()
            => QueryProvider.Execute<IEnumerable<TEntity>>(Expression).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => QueryProvider.Execute<IEnumerable>(Expression).GetEnumerator();

        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetEnumerator()
            => QueryProvider.ExecuteAsync<TEntity>(Expression).GetEnumerator();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Context
{
    public class EntitySet<TEntity> 
        : IQueryable<TEntity>, IAsyncEnumerableAccessor<TEntity>
        where TEntity : class
    {
        private readonly EntityQueryable<TEntity> _entityQueryable;
        private readonly IDataAgentFactory _dataAgentFactory;

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => _entityQueryable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _entityQueryable.GetEnumerator();

        IAsyncEnumerable<TEntity> IAsyncEnumerableAccessor<TEntity>.AsyncEnumerable => _entityQueryable;
        
        Type IQueryable.ElementType => _entityQueryable.ElementType;

        Expression IQueryable.Expression => _entityQueryable.Expression;

        IQueryProvider IQueryable.Provider => _entityQueryable.Provider;
        
        public EntitySet(EntityQueryable<TEntity> entityQueryable, IDataAgentFactory dataAgentFactory)
        {
            _entityQueryable = entityQueryable;
            _dataAgentFactory = dataAgentFactory ?? throw new ArgumentNullException(nameof(dataAgentFactory));
        }

        public Task<TEntity> CreateAsync(
            DataContext<TEntity> context, 
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            => _dataAgentFactory.GetCreateDataAgent().CreateAsync(context.WithEntitySet(this), entity, token);
        
        public Task<TEntity> UpdateAsync(
            DataContext<TEntity> context,
            TEntity entity, 
            CancellationToken token = default(CancellationToken))
            => _dataAgentFactory.GetUpdateDataAgent().UpdateAsync(context.WithEntitySet(this), entity, token);

        public Task<TEntity> DeleteAsync(
            DataContext<TEntity> context, 
            TEntity entity, 
            CancellationToken token = default(CancellationToken))
            => _dataAgentFactory.GetDeleteDataAgent().DeleteAsync(context.WithEntitySet(this), entity, token);

        public Task<TEntity[]> CreateAsync(
            DataContext<TEntity> context, 
            IEnumerable<TEntity> entities, 
            CancellationToken token = default(CancellationToken))
            => _dataAgentFactory.GetBulkCreateDataAgent().CreateAsync(context.WithEntitySet(this), entities, token);

        public Task<TEntity[]> UpdateAsync(
            DataContext<TEntity> context, 
            IEnumerable<TEntity> entities, 
            CancellationToken token = default(CancellationToken))
            => _dataAgentFactory.GetBulkUpdateDataAgent().UpdateAsync(context.WithEntitySet(this), entities, token);

        public Task<TEntity[]> DeleteAsync(
            DataContext<TEntity> context, 
            IEnumerable<TEntity> entities, 
            CancellationToken token = default(CancellationToken))
            => _dataAgentFactory.GetBulkDeleteDataAgent().DeleteAsync(context.WithEntitySet(this), entities, token);
    }
}

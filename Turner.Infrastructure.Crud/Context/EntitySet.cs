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
        private readonly IDataAgent _dataAgent;

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => _entityQueryable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _entityQueryable.GetEnumerator();

        IAsyncEnumerable<TEntity> IAsyncEnumerableAccessor<TEntity>.AsyncEnumerable => _entityQueryable;
        
        Type IQueryable.ElementType => _entityQueryable.ElementType;

        Expression IQueryable.Expression => _entityQueryable.Expression;

        IQueryProvider IQueryable.Provider => _entityQueryable.Provider;
        
        public EntitySet(EntityQueryable<TEntity> entityQueryable, IDataAgent dataAgent)
        {
            _entityQueryable = entityQueryable;
            _dataAgent = dataAgent ?? throw new ArgumentNullException(nameof(dataAgent));
        }

        public Task<TEntity> CreateAsync(TEntity entity, 
            CancellationToken token = default(CancellationToken))
            => _dataAgent.CreateAsync(this, entity, token);

        public Task<TEntity> UpdateAsync(TEntity entity, 
            CancellationToken token = default(CancellationToken))
            => _dataAgent.UpdateAsync(this, entity, token);

        public Task<TEntity> DeleteAsync(TEntity entity, 
            CancellationToken token = default(CancellationToken))
            => _dataAgent.DeleteAsync(this, entity, token);

        public Task<TEntity[]> CreateAsync(IEnumerable<TEntity> entities, 
            CancellationToken token = default(CancellationToken))
            => _dataAgent.CreateAsync(this, entities, token);

        public Task<TEntity[]> UpdateAsync(IEnumerable<TEntity> entities, 
            CancellationToken token = default(CancellationToken))
            => _dataAgent.UpdateAsync(this, entities, token);

        public Task<TEntity[]> DeleteAsync(IEnumerable<TEntity> entities, 
            CancellationToken token = default(CancellationToken))
            => _dataAgent.DeleteAsync(this, entities, token);
    }
}

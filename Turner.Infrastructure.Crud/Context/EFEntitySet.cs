using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Context
{
    public class EFEntitySet<TEntity> : IEntitySet<TEntity>, IAsyncEnumerable<TEntity>
        where TEntity : class
    {
        private DbSet<TEntity> _set;

        public Type ElementType => _set.AsQueryable().ElementType;

        public Expression Expression => _set.AsQueryable().Expression;

        public IQueryProvider Provider => _set.AsQueryable().Provider;

        public IEnumerator<TEntity> GetEnumerator() => _set.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _set.AsEnumerable().GetEnumerator();

        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetEnumerator() => _set.ToAsyncEnumerable().GetEnumerator();

        internal static EFEntitySet<TEntity> From(DbSet<TEntity> set)
        {
            return new EFEntitySet<TEntity>
            {
                _set = set
            };
        }
 
        public virtual Task<TEntity> CreateAsync(TEntity entity,
            CancellationToken token = default(CancellationToken))
        {
            var trackedEntity = _set.Add(entity);

            token.ThrowIfCancellationRequested();
            return Task.FromResult(trackedEntity.Entity);
        }

        public virtual async Task<TEntity[]> CreateAsync(IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
        {
            var result = new List<TEntity>();

            foreach (var entity in entities)
            {
                result.Add(await CreateAsync(entity, token));
                token.ThrowIfCancellationRequested();
            }

            return result.ToArray();
        }

        public virtual Task<TEntity> UpdateAsync(TEntity entity,
            CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();

            return Task.FromResult(entity);
        }

        public virtual Task<TEntity[]> UpdateAsync(IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();

            return Task.FromResult(entities.ToArray());
        }
        
        public virtual Task<TEntity> DeleteAsync(TEntity entity,
            CancellationToken token = default(CancellationToken))
        {
            var trackedEntity = _set.Remove(entity);
            token.ThrowIfCancellationRequested();
            
            return Task.FromResult(trackedEntity.Entity);
        }

        public virtual async Task<TEntity[]> DeleteAsync(IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
        {
            var result = new List<TEntity>();

            foreach (var entity in entities)
            {
                result.Add(await DeleteAsync(entity, token));
                token.ThrowIfCancellationRequested();
            }

            return result.ToArray();
        }

        public virtual async Task DeleteAsync(IQueryable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
        {
            _set.RemoveRange(await entities.ToArrayAsync(token));
        }
    }
}

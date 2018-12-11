using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Algorithms
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

        public static EFEntitySet<TEntity> From(DbSet<TEntity> set)
        {
            var entitySet = new EFEntitySet<TEntity>();
            entitySet._set = set;

            return entitySet;
        }
        
        public virtual TEntity Create(TEntity entity) => _set.Add(entity).Entity;

        public virtual TEntity[] Create(IEnumerable<TEntity> entities) => 
            entities.Select(Create).ToArray();

        public virtual async Task<TEntity> CreateAsync(TEntity entity,
            CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();
            var trackedEntity = await _set.AddAsync(entity, token).Configure();

            token.ThrowIfCancellationRequested();
            return trackedEntity.Entity;
        }

        public virtual async Task<TEntity[]> CreateAsync(IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
        {
            var result = new List<TEntity>();

            foreach (var entity in entities)
            {
                token.ThrowIfCancellationRequested();
                result.Add(await CreateAsync(entity));
            }

            return result.ToArray();
        }

        public virtual TEntity Update(TEntity entity) => entity;

        public virtual TEntity[] Update(IEnumerable<TEntity> entities) =>
            entities.Select(Update).ToArray();

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

        public virtual TEntity Delete(TEntity entity) => _set.Remove(entity).Entity;

        public virtual TEntity[] Delete(IEnumerable<TEntity> entities) =>
            entities.Select(Delete).ToArray();

        public virtual Task<TEntity> DeleteAsync(TEntity entity,
            CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();
            var trackedEntity = _set.Remove(entity);
            
            return Task.FromResult(trackedEntity.Entity);
        }

        public virtual async Task<TEntity[]> DeleteAsync(IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
        {
            var result = new List<TEntity>();

            foreach (var entity in entities)
            {
                token.ThrowIfCancellationRequested();
                result.Add(await DeleteAsync(entity));
            }

            return result.ToArray();
        }
    }
}

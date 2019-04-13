using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.EntityFrameworkCore
{
    public class EntityFrameworkSingleSetOperator : ISingleSetOperator
    {
        public Task<TEntity> CreateAsync<TEntity>(EntitySet<TEntity> entitySet, 
            TEntity entity, 
            CancellationToken token = default(CancellationToken)) 
            where TEntity : class
        {
            var set = entitySet as EntityFrameworkEntitySet<TEntity>;
            var trackedEntity = set.Set.Add(entity);

            token.ThrowIfCancellationRequested();

            return Task.FromResult(trackedEntity.Entity);
        }

        public Task<TEntity> UpdateAsync<TEntity>(EntitySet<TEntity> entitySet, 
            TEntity entity, 
            CancellationToken token = default(CancellationToken)) 
            where TEntity : class
        {
            token.ThrowIfCancellationRequested();

            return Task.FromResult(entity);
        }

        public Task<TEntity> DeleteAsync<TEntity>(EntitySet<TEntity> entitySet, 
            TEntity entity, 
            CancellationToken token = default(CancellationToken)) 
            where TEntity : class
        {
            var set = entitySet as EntityFrameworkEntitySet<TEntity>;
            var trackedEntity = set.Set.Remove(entity);

            token.ThrowIfCancellationRequested();

            return Task.FromResult(trackedEntity.Entity);
        }
    }

    public class EntityFrameworkBulkSetOperator : IBulkSetOperator
    {
        public Task<TEntity[]> CreateAsync<TEntity>(EntitySet<TEntity> entitySet, 
            IEnumerable<TEntity> entities, 
            CancellationToken token = default(CancellationToken)) 
            where TEntity : class
        {
            var set = entitySet as EntityFrameworkEntitySet<TEntity>;
            var result = new List<TEntity>();

            foreach (var entity in entities)
            {
                var trackedEntity = set.Set.Add(entity);
                result.Add(trackedEntity.Entity);

                token.ThrowIfCancellationRequested();
            }

            return Task.FromResult(result.ToArray());
        }

        public Task<TEntity[]> DeleteAsync<TEntity>(EntitySet<TEntity> entitySet, 
            IEnumerable<TEntity> entities, 
            CancellationToken token = default(CancellationToken)) 
            where TEntity : class
        {
            var set = entitySet as EntityFrameworkEntitySet<TEntity>;
            var result = new List<TEntity>();

            foreach (var entity in entities)
            {
                var trackedEntity = set.Set.Remove(entity);
                result.Add(trackedEntity.Entity);

                token.ThrowIfCancellationRequested();
            }

            return Task.FromResult(result.ToArray());
        }

        public Task<TEntity[]> UpdateAsync<TEntity>(EntitySet<TEntity> entitySet, 
            IEnumerable<TEntity> entities, 
            CancellationToken token = default(CancellationToken)) 
            where TEntity : class
        {
            token.ThrowIfCancellationRequested();

            return Task.FromResult(entities.ToArray());
        }
    }
}

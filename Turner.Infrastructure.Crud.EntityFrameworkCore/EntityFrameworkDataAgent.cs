using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.EntityFrameworkCore
{
    public class EntityFrameworkDataAgent
        : ICreateDataAgent,
          IUpdateDataAgent,
          IDeleteDataAgent,
          IBulkCreateDataAgent,
          IBulkUpdateDataAgent,
          IBulkDeleteDataAgent
    {
        public Task<TEntity> CreateAsync<TEntity>(DataContext<TEntity> context,
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var set = context.EntitySet as EntityFrameworkEntitySet<TEntity>;
            var trackedEntity = set.Context.Set<TEntity>().Add(entity);

            token.ThrowIfCancellationRequested();

            return Task.FromResult(trackedEntity.Entity);
        }

        public Task<TEntity> UpdateAsync<TEntity>(DataContext<TEntity> context,
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            token.ThrowIfCancellationRequested();

            return Task.FromResult(entity);
        }

        public Task<TEntity> DeleteAsync<TEntity>(DataContext<TEntity> context,
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var set = context.EntitySet as EntityFrameworkEntitySet<TEntity>;
            var trackedEntity = set.Context.Set<TEntity>().Remove(entity);

            token.ThrowIfCancellationRequested();

            return Task.FromResult(trackedEntity.Entity);
        }

        public Task<TEntity[]> CreateAsync<TEntity>(DataContext<TEntity> context,
            IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var set = context.EntitySet as EntityFrameworkEntitySet<TEntity>;
            var contextSet = set.Context.Set<TEntity>();
            var result = new List<TEntity>();

            foreach (var entity in entities)
            {
                var trackedEntity = contextSet.Add(entity);
                result.Add(trackedEntity.Entity);

                token.ThrowIfCancellationRequested();
            }

            return Task.FromResult(result.ToArray());
        }

        public Task<TEntity[]> UpdateAsync<TEntity>(DataContext<TEntity> context,
            IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            token.ThrowIfCancellationRequested();

            return Task.FromResult(entities.ToArray());
        }

        public Task<TEntity[]> DeleteAsync<TEntity>(DataContext<TEntity> context,
            IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var set = context.EntitySet as EntityFrameworkEntitySet<TEntity>;
            var contextSet = set.Context.Set<TEntity>();
            var result = new List<TEntity>();

            foreach (var entity in entities)
            {
                var trackedEntity = contextSet.Remove(entity);
                result.Add(trackedEntity.Entity);

                token.ThrowIfCancellationRequested();
            }

            return Task.FromResult(result.ToArray());
        }
    }
}

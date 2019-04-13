using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Tests.Fakes;

namespace Turner.Infrastructure.Crud.Tests.ContextTests
{
    public class InMemoryDataAgent : IDataAgent
    {
        public Task<TEntity> CreateAsync<TEntity>(EntitySet<TEntity> entitySet,
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var set = entitySet as InMemorySet<TEntity>;

            if (entity is IEntity entityWithId)
                entityWithId.Id = set.Id++;

            set.Items.Add(entity);

            return Task.FromResult(entity);
        }

        public Task<TEntity> UpdateAsync<TEntity>(EntitySet<TEntity> entitySet,
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
            => Task.FromResult(entity);

        public Task<TEntity> DeleteAsync<TEntity>(EntitySet<TEntity> entitySet,
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var set = entitySet as InMemorySet<TEntity>;

            set.Items.Remove(entity);

            return Task.FromResult(entity);
        }

        public Task<TEntity[]> CreateAsync<TEntity>(EntitySet<TEntity> entitySet,
            IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var set = entitySet as InMemorySet<TEntity>;
            var result = entities.ToArray();

            foreach (var entity in entities)
            {
                if (entity is IEntity entityWithId)
                    entityWithId.Id = set.Id++;

                set.Items.Add(entity);
            }

            return Task.FromResult(result);
        }

        public Task<TEntity[]> UpdateAsync<TEntity>(EntitySet<TEntity> entitySet,
            IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
            => Task.FromResult(entities.ToArray());

        public Task<TEntity[]> DeleteAsync<TEntity>(EntitySet<TEntity> entitySet,
            IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var set = entitySet as InMemorySet<TEntity>;
            var result = entities.ToArray();

            foreach (var entity in result)
                set.Items.Remove(entity);

            return Task.FromResult(result);
        }
    }
}

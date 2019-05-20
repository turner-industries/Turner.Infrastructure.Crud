using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Tests.Fakes;

namespace Turner.Infrastructure.Crud.Tests.ContextTests
{
    public class InMemoryDataAgent
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
            var set = context.EntitySet as InMemorySet<TEntity>;

            if (entity is IEntity entityWithId)
                entityWithId.Id = set.Id++;

            set.Items.Add(entity);

            return Task.FromResult(entity);
        }

        public Task<TEntity> UpdateAsync<TEntity>(DataContext<TEntity> context,
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
            => Task.FromResult(entity);

        public Task<TEntity> DeleteAsync<TEntity>(DataContext<TEntity> context,
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var set = context.EntitySet as InMemorySet<TEntity>;

            set.Items.Remove(entity);

            return Task.FromResult(entity);
        }

        public Task<TEntity[]> CreateAsync<TEntity>(DataContext<TEntity> context,
            IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var set = context.EntitySet as InMemorySet<TEntity>;
            var result = entities.ToArray();

            foreach (var entity in result)
            {
                if (entity is IEntity entityWithId)
                    entityWithId.Id = set.Id++;

                set.Items.Add(entity);
            }

            return Task.FromResult(result);
        }

        public Task<TEntity[]> UpdateAsync<TEntity>(DataContext<TEntity> context,
            IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
            => Task.FromResult(entities.ToArray());

        public Task<TEntity[]> DeleteAsync<TEntity>(DataContext<TEntity> context,
            IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var set = context.EntitySet as InMemorySet<TEntity>;
            var result = entities.ToArray();

            foreach (var entity in result)
                set.Items.Remove(entity);

            return Task.FromResult(result);
        }
    }
}

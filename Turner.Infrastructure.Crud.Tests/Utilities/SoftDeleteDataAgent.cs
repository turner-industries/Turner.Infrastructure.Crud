using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.EntityFrameworkCore;
using Turner.Infrastructure.Crud.EntityFrameworkExtensions.Configuration;
using Turner.Infrastructure.Crud.EntityFrameworkExtensions.Extensions;
using Turner.Infrastructure.Crud.Tests.Fakes;

namespace Turner.Infrastructure.Crud.Tests.Utilities
{
    public class SoftDeleteDataAgent : IDeleteDataAgent, IBulkDeleteDataAgent
    {
        public Task<TEntity> DeleteAsync<TEntity>(DataContext<TEntity> context,
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var set = context.EntitySet as EntityFrameworkEntitySet<TEntity>;
            var entry = set.Context.Entry(entity);

            if (entity is IEntity ientity)
            {
                ientity.IsDeleted = true;
                entry.State = EntityState.Modified;
            }
            else
            {
                entry.State = EntityState.Deleted;
            }

            token.ThrowIfCancellationRequested();

            return Task.FromResult(entry.Entity);
        }

        public async Task<TEntity[]> DeleteAsync<TEntity>(DataContext<TEntity> context,
            IEnumerable<TEntity> items,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            token.ThrowIfCancellationRequested();

            var set = context.EntitySet as EntityFrameworkEntitySet<TEntity>;
            var entities = items.ToArray();

            if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
            {
                var entries = set.Context.ChangeTracker
                    .Entries()
                    .Where(x => entities.Contains(x.Entity) && x.State == EntityState.Deleted)
                    .ToArray();

                foreach (var entry in entries)
                    entry.State = EntityState.Detached;

                if (set.Context.ChangeTracker.Entries().Any(x => x.Entity is TEntity))
                    await set.Context.SaveChangesAsync(token);

                foreach (var entity in entities.Cast<IEntity>())
                    entity.IsDeleted = true;

                await set.Context.BulkUpdateAsync(entities,
                    operation => operation.Configure(BulkConfigurationType.Delete, context),
                    token);
            }
            else
            {
                await set.Context.BulkDeleteAsync(entities,
                    operation => operation.Configure(BulkConfigurationType.Delete, context),
                    token);
            }

            return entities;
        }
    }
}

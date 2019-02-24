using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Turner.Infrastructure.Crud.Context
{
    public class EntityFrameworkContext : IEntityContext
    {
        private readonly DbContext _context;

        public EntityFrameworkContext(DbContext context)
        {
            _context = context;
        }

        public virtual IEntitySet<TEntity> EntitySet<TEntity>()
            where TEntity : class
        {
            return EntityFrameworkEntitySet<TEntity>.From(_context.Set<TEntity>());
        }

        public virtual async Task<int> ApplyChangesAsync(CancellationToken token = default(CancellationToken))
        {
            var result = await _context.SaveChangesAsync(token).ConfigureAwait(false);

            token.ThrowIfCancellationRequested();
            return result;
        }

        public virtual Task<T> SingleOrDefaultAsync<T>(IQueryable<T> entities,
            CancellationToken token = default(CancellationToken))
            => entities.SingleOrDefaultAsync(token);

        public virtual Task<T> SingleOrDefaultAsync<T>(IQueryable<T> entities,
            Expression<Func<T, bool>> predicate,
            CancellationToken token = default(CancellationToken))
            => entities.SingleOrDefaultAsync(predicate, token);

        public virtual Task<List<T>> ToListAsync<T>(IQueryable<T> entities,
            CancellationToken token = default(CancellationToken))
            => entities.ToListAsync(token);

        public virtual Task<T[]> ToArrayAsync<T>(IQueryable<T> entities,
            CancellationToken token = default(CancellationToken))
            => entities.ToArrayAsync(token);

        public virtual Task<int> CountAsync<T>(IQueryable<T> entities,
            CancellationToken token = default(CancellationToken))
            => entities.CountAsync();
    }
}

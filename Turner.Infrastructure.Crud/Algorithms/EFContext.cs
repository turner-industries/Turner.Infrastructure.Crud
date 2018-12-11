using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Algorithms
{
    public class EFContext : IEntityContext
    {
        private readonly DbContext _context;

        public EFContext(DbContext context)
        {
            _context = context;
        }

        public virtual IEntitySet<TEntity> EntitySet<TEntity>()
            where TEntity : class
        {
            return EFEntitySet<TEntity>.From(_context.Set<TEntity>());
        }

        public virtual int ApplyChanges() => _context.SaveChanges();

        public virtual async Task<int> ApplyChangesAsync(CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();
            var result = await _context.SaveChangesAsync(token).Configure();

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

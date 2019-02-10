using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Context
{
    public interface IEntityContext
    {
        IEntitySet<TEntity> EntitySet<TEntity>() where TEntity : class;

        Task<int> ApplyChangesAsync(CancellationToken token = default(CancellationToken));

        Task<T> SingleOrDefaultAsync<T>(IQueryable<T> entities,
            CancellationToken token = default(CancellationToken));

        Task<T> SingleOrDefaultAsync<T>(IQueryable<T> entities,
            Expression<Func<T, bool>> predicate,
            CancellationToken token = default(CancellationToken));

        Task<List<T>> ToListAsync<T>(IQueryable<T> entities,
            CancellationToken token = default(CancellationToken));

        Task<T[]> ToArrayAsync<T>(IQueryable<T> entities,
            CancellationToken token = default(CancellationToken));

        Task<int> CountAsync<T>(IQueryable<T> entities,
            CancellationToken token = default(CancellationToken));
    }
}

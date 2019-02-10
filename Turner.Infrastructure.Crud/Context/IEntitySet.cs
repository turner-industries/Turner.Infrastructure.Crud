using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Context
{
    public interface IEntitySet<TEntity> : IQueryable<TEntity>
    {
        Task<TEntity> CreateAsync(TEntity entity, CancellationToken token = default(CancellationToken));

        Task<TEntity[]> CreateAsync(IEnumerable<TEntity> entities, CancellationToken token = default(CancellationToken));

        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken token = default(CancellationToken));

        Task<TEntity[]> UpdateAsync(IEnumerable<TEntity> entities, CancellationToken token = default(CancellationToken));

        Task<TEntity> DeleteAsync(TEntity entity, CancellationToken token = default(CancellationToken));

        Task<TEntity[]> DeleteAsync(IEnumerable<TEntity> entities, CancellationToken token = default(CancellationToken));

        Task DeleteAsync(IQueryable<TEntity> entities, CancellationToken token = default(CancellationToken));
    }
}

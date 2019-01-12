using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Algorithms
{
    public interface IEntitySet<TEntity> : IQueryable<TEntity>
    {
        TEntity Create(TEntity entity);

        TEntity[] Create(IEnumerable<TEntity> entities);
        
        Task<TEntity> CreateAsync(TEntity entity, CancellationToken token = default(CancellationToken));

        Task<TEntity[]> CreateAsync(IEnumerable<TEntity> entities, CancellationToken token = default(CancellationToken));

        TEntity Update(TEntity entity);

        TEntity[] Update(IEnumerable<TEntity> entities);

        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken token = default(CancellationToken));

        Task<TEntity[]> UpdateAsync(IEnumerable<TEntity> entities, CancellationToken token = default(CancellationToken));

        TEntity Delete(TEntity entity);

        TEntity[] Delete(IEnumerable<TEntity> entities);

        Task<TEntity> DeleteAsync(TEntity entity, CancellationToken token = default(CancellationToken));

        Task<TEntity[]> DeleteAsync(IEnumerable<TEntity> entities, CancellationToken token = default(CancellationToken));

        Task DeleteAsync(IQueryable<TEntity> entities, CancellationToken token = default(CancellationToken));
    }
}

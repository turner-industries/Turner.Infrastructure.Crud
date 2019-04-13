using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Context
{
    public interface ISingleSetOperator
    {
        Task<TEntity> CreateAsync<TEntity>(EntitySet<TEntity> entitySet,
            TEntity entity, 
            CancellationToken token = default(CancellationToken))
            where TEntity : class;

        Task<TEntity> UpdateAsync<TEntity>(EntitySet<TEntity> entitySet,
            TEntity entity, 
            CancellationToken token = default(CancellationToken))
            where TEntity : class;

        Task<TEntity> DeleteAsync<TEntity>(EntitySet<TEntity> entitySet,
            TEntity entity, 
            CancellationToken token = default(CancellationToken))
            where TEntity : class;
    }
}

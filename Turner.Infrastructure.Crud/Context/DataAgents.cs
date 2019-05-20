using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Context
{
    public interface ICreateDataAgent
    {
        Task<TEntity> CreateAsync<TEntity>(DataContext<TEntity> context,
                TEntity entity,
                CancellationToken token = default(CancellationToken))
                where TEntity : class;
    }

    public interface IUpdateDataAgent
    {
        Task<TEntity> UpdateAsync<TEntity>(DataContext<TEntity> context,
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : class;
    }

    public interface IDeleteDataAgent
    {
        Task<TEntity> DeleteAsync<TEntity>(DataContext<TEntity> context,
            TEntity entity,
            CancellationToken token = default(CancellationToken))
            where TEntity : class;
    }

    public interface IBulkCreateDataAgent
    {
        Task<TEntity[]> CreateAsync<TEntity>(DataContext<TEntity> context,
            IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
            where TEntity : class;
    }

    public interface IBulkUpdateDataAgent
    {
        Task<TEntity[]> UpdateAsync<TEntity>(DataContext<TEntity> context,
            IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
            where TEntity : class;
    }

    public interface IBulkDeleteDataAgent
    {
        Task<TEntity[]> DeleteAsync<TEntity>(DataContext<TEntity> context,
            IEnumerable<TEntity> entities,
            CancellationToken token = default(CancellationToken))
            where TEntity : class;
    }
}

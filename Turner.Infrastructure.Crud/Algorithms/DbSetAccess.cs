using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Algorithms
{
    public interface IDbSetAccess
    {
        TEntity Create<TEntity>(TEntity entity, DbSet<TEntity> set)
            where TEntity : class;

        Task<TEntity> CreateAsync<TEntity>(TEntity entity, 
            DbSet<TEntity> set, 
            CancellationToken token = default(CancellationToken))
            where TEntity : class;
    }

    public class StandardDbSetAccess : IDbSetAccess
    {
        public TEntity Create<TEntity>(TEntity entity, DbSet<TEntity> set)
            where TEntity : class
        {
            return set.Add(entity).Entity;
        }

        public async Task<TEntity> CreateAsync<TEntity>(TEntity entity, 
            DbSet<TEntity> set,
            CancellationToken token = default(CancellationToken))
            where TEntity : class
        {
            var trackedEntity = await set.AddAsync(entity, token).Configure();
            return trackedEntity.Entity;
        }
    }
}

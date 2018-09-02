using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Algorithms
{
    public interface IContextAccess
    {
        DbSet<TEntity> GetEntities<TEntity>(DbContext context)
            where TEntity : class;

        Task ApplyChangesAsync(DbContext context);
    }

    public class StandardContextAccess : IContextAccess
    {
        public DbSet<TEntity> GetEntities<TEntity>(DbContext context)
            where TEntity : class
        {
            return context.Set<TEntity>();
        }

        public async Task ApplyChangesAsync(DbContext context)
        {
            await context.SaveChangesAsync();
        }
    }
}

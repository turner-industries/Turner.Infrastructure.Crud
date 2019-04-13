using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.EntityFrameworkCore
{
    public class EntityFrameworkContext : IEntityContext
    {
        private readonly DbContext _context;
        private readonly IDataAgent _dataAgent;

        public EntityFrameworkContext(DbContext context, 
            IDataAgent dataAgent)
        {
            _context = context;
            _dataAgent = dataAgent;
        }

        public virtual EntitySet<TEntity> Set<TEntity>()
            where TEntity : class
            => new EntityFrameworkEntitySet<TEntity>(_context.Set<TEntity>(), _dataAgent);

        public virtual async Task<int> ApplyChangesAsync(CancellationToken token = default(CancellationToken))
        {
            var result = await _context.SaveChangesAsync(token).ConfigureAwait(false);

            token.ThrowIfCancellationRequested();
            return result;
        }
    }
}

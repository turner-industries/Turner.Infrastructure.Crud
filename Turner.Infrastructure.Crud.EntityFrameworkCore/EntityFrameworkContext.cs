using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.EntityFrameworkCore
{
    public class EntityFrameworkContext : IEntityContext
    {
        private readonly DbContext _context;
        private readonly ISingleSetOperator _singleSetOperator;
        private readonly IBulkSetOperator _bulkSetOperator;

        public EntityFrameworkContext(DbContext context, 
            ISingleSetOperator singleSetOperator,
            IBulkSetOperator bulkSetOperator)
        {
            _context = context;
            _singleSetOperator = singleSetOperator;
            _bulkSetOperator = bulkSetOperator;
        }

        public virtual EntitySet<TEntity> Set<TEntity>()
            where TEntity : class
        {
            return new EntityFrameworkEntitySet<TEntity>(
                _context.Set<TEntity>(), 
                _singleSetOperator, 
                _bulkSetOperator);
        }

        public virtual async Task<int> ApplyChangesAsync(CancellationToken token = default(CancellationToken))
        {
            var result = await _context.SaveChangesAsync(token).ConfigureAwait(false);

            token.ThrowIfCancellationRequested();
            return result;
        }
    }
}

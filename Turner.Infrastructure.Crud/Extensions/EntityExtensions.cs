using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;

namespace Turner.Infrastructure.Crud.Extensions
{
    internal static class EntityExtensions
    {
        internal static async Task<TResult> CreateResult<TEntity, TResult>(this TEntity entity,
            ICrudRequestConfig config,
            CancellationToken token)
            where TEntity : class
        {
            var createResult = config.GetResultCreatorFor<TEntity, TResult>();

            var result = await createResult(entity, token).Configure();
            token.ThrowIfCancellationRequested();

            return result;
        }

        internal static async Task<TResult[]> CreateResults<TEntity, TResult>(this TEntity[] entities,
            ICrudRequestConfig config,
            CancellationToken token)
            where TEntity : class
        {
            var createResult = config.GetResultCreatorFor<TEntity, TResult>();

            var results = await Task.WhenAll(entities.Select(x => createResult(x, token))).Configure();
            token.ThrowIfCancellationRequested();
            
            return results;
        }
    }
}

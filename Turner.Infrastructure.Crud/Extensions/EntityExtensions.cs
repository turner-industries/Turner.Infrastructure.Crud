using System;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Extensions
{
    internal static class EntityExtensions
    {
        internal static async Task<TResult> CreateResult<TEntity, TResult>(this TEntity entity,
            Func<TEntity, CancellationToken, Task<TResult>> createResult,
            CancellationToken token)
            where TEntity : class
        {
            var result = await createResult(entity, token).Configure();
            token.ThrowIfCancellationRequested();

            return result;
        }
    }
}

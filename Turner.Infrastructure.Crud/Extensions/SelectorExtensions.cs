using AutoMapper;
using AutoMapper.QueryableExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;

namespace Turner.Infrastructure.Crud
{
    internal static class QueryableExtensions
    {
        internal static async Task<List<TOut>> ProjectResults<TEntity, TOut>(
            this IQueryable<TEntity> entities, 
            IEntityContext context, 
            RequestOptions options,
            CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();

            if (options.UseProjection)
            {
                return await context.ToListAsync(entities.ProjectTo<TOut>(), token).Configure();
            }
            else
            {
                var resultEntities = await context.ToListAsync(entities, token).Configure();
                token.ThrowIfCancellationRequested();

                return Mapper.Map<List<TOut>>(resultEntities);
            }
        }
    }
}

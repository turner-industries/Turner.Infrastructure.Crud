using System.Collections.Generic;
using System.Linq;

namespace Turner.Infrastructure.Crud.Extensions
{
    internal static class UtilityQueryableExtensions
    {
        internal static IQueryable<TEntity> FilterWith<TEntity>(this IQueryable<TEntity> entities,
            object request, 
            IEnumerable<IBoxedFilter> filters)
            where TEntity : class
        {
            return filters.Aggregate(entities, (current, filter)
                => filter.Filter(request, current).Cast<TEntity>());
        }

        internal static IQueryable<TEntity> SortWith<TEntity>(this IQueryable<TEntity> entities,
            object request,
            IBoxedSorter sorter)
            where TEntity : class
        {
            return sorter?.Sort(request, entities).Cast<TEntity>() ?? entities;
        }

        internal static IQueryable<TEntity> SelectWith<TEntity>(this IQueryable<TEntity> entities,
            object request,
            ISelector selector)
            where TEntity : class
        {
            return entities.Where(selector.Get<TEntity>()(request));
        }
    }
}

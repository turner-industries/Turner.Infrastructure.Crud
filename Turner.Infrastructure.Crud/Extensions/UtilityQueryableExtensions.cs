using System.Linq;
using Turner.Infrastructure.Crud.Configuration;

namespace Turner.Infrastructure.Crud.Extensions
{
    internal static class UtilityQueryableExtensions
    {
        internal static IQueryable<TEntity> FilterWith<TEntity>(this IQueryable<TEntity> entities,
            object request, 
            IRequestConfig config)
            where TEntity : class
        {
            var filters = config.GetFiltersFor<TEntity>();

            return filters.Aggregate(entities, (current, filter)
                => filter.Filter(request, current).Cast<TEntity>());
        }

        internal static IQueryable<TEntity> SortWith<TEntity>(this IQueryable<TEntity> entities,
            object request,
            IRequestConfig config)
            where TEntity : class
        {
            var sorter = config.GetSorterFor<TEntity>();

            return sorter?.Sort(request, entities).Cast<TEntity>() ?? entities;
        }

        internal static IQueryable<TEntity> SelectWith<TEntity>(this IQueryable<TEntity> entities,
            object request,
            IRequestConfig config)
            where TEntity : class
        {
            var selector = config.GetSelectorFor<TEntity>().Get<TEntity>();

            return entities.Where(selector(request));
        }
    }
}

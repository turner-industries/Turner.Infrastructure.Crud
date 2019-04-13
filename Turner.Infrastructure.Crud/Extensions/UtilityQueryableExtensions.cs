﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Requests;

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

        internal static async Task RunRequestHooks(this ICrudRequest request, 
            List<IBoxedRequestHook> hooks, 
            CancellationToken ct)
        {
            foreach (var hook in hooks)
            {
                await hook.Run(request, ct).Configure();
                ct.ThrowIfCancellationRequested();
            }
        }

        internal static async Task<object[]> RunItemHooks(this IBulkRequest request,
            List<IBoxedItemHook> hooks,
            object[] items,
            CancellationToken ct)
        {
            foreach (var hook in hooks)
            {
                for (var i = 0; i < items.Length; ++i)
                {
                    items[i] = await hook.Run(request, items[i], ct).Configure();
                    ct.ThrowIfCancellationRequested();
                }
            }

            return items;
        }
        
        internal static async Task RunEntityHooks(this ICrudRequest request,
            List<IBoxedEntityHook> hooks,
            object entity,
            CancellationToken ct)
        {
            foreach (var hook in hooks)
            {
                await hook.Run(request, entity, ct).Configure();
                ct.ThrowIfCancellationRequested();
            }
        }

        internal static async Task RunEntityHooks(this ICrudRequest request,
            List<IBoxedEntityHook> hooks,
            IEnumerable<object> entities,
            CancellationToken ct)
        {
            foreach (var entity in entities)
            {
                foreach (var hook in hooks)
                {
                    await hook.Run(request, entity, ct).Configure();
                    ct.ThrowIfCancellationRequested();
                }
            }
        }
        
        internal static async Task<T> RunResultHooks<T>(this ICrudRequest request,
            List<IBoxedResultHook> hooks,
            T result,
            CancellationToken ct)
        {
            foreach (var hook in hooks)
            {
                result = (T)await hook.Run(request, result, ct).Configure();
                ct.ThrowIfCancellationRequested();
            }

            return result;
        }

        internal static async Task<List<T>> RunResultHooks<T>(this ICrudRequest request,
            List<IBoxedResultHook> hooks,
            List<T> results,
            CancellationToken ct)
        {
            foreach (var hook in hooks)
            {
                for (var i = 0; i < results.Count; ++i)
                {
                    results[i] = (T)await hook.Run(request, results[i], ct).Configure();
                    ct.ThrowIfCancellationRequested();
                }
            }

            return results;
        }
    }
}

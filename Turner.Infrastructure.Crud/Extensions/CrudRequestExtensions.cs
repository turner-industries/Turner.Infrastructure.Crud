using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Requests;

namespace Turner.Infrastructure.Crud.Extensions
{
    internal static class CrudRequestExtensions
    {
        private static bool IsHookFailure(Exception e)
            => !(e is OperationCanceledException);

        private static string HookFailureReason(string hookType)
            => $"A request '{hookType} hook' failed while processing the request.";

        internal static async Task RunRequestHooks(this ICrudRequest request,
            ICrudRequestConfig config,
            CancellationToken ct)
        {
            var hooks = config.GetRequestHooks();

            foreach (var hook in hooks)
            {
                try
                {
                    await hook.Run(request, ct).Configure();
                    ct.ThrowIfCancellationRequested();
                }
                catch (Exception e) when (IsHookFailure(e))
                {
                    throw new CrudHookFailedException(HookFailureReason("request"), e)
                    {
                        HookProperty = hook
                    };
                }
            }
        }

        internal static async Task<object[]> RunItemHooks<TEntity>(this IBulkRequest request,
            ICrudRequestConfig config,
            object[] items,
            CancellationToken ct)
            where TEntity : class
        {
            var hooks = config.GetItemHooksFor<TEntity>();

            foreach (var hook in hooks)
            {
                for (var i = 0; i < items.Length; ++i)
                {
                    try
                    {
                        items[i] = await hook.Run(request, items[i], ct).Configure();
                        ct.ThrowIfCancellationRequested();
                    }
                    catch (Exception e) when (IsHookFailure(e))
                    {
                        throw new CrudHookFailedException(HookFailureReason("item"), e)
                        {
                            HookProperty = hook
                        };
                    }
                }
            }

            return items;
        }

        internal static async Task RunEntityHooks<TEntity>(this ICrudRequest request,
            ICrudRequestConfig config,
            object entity,
            CancellationToken ct)
            where TEntity : class
        {
            var hooks = config.GetEntityHooksFor<TEntity>();

            foreach (var hook in hooks)
            {
                try
                {
                    await hook.Run(request, entity, ct).Configure();
                    ct.ThrowIfCancellationRequested();
                }
                catch (Exception e) when (IsHookFailure(e))
                {
                    throw new CrudHookFailedException(HookFailureReason("entity"), e)
                    {
                        HookProperty = hook
                    };
                }
            }
        }

        internal static async Task RunEntityHooks<TEntity>(this ICrudRequest request,
            ICrudRequestConfig config,
            IEnumerable<object> entities,
            CancellationToken ct)
            where TEntity : class
        {
            var hooks = config.GetEntityHooksFor<TEntity>();

            foreach (var entity in entities)
            {
                foreach (var hook in hooks)
                {
                    try
                    {
                        await hook.Run(request, entity, ct).Configure();
                        ct.ThrowIfCancellationRequested();
                    }
                    catch (Exception e) when (IsHookFailure(e))
                    {
                        throw new CrudHookFailedException(HookFailureReason("entity"), e)
                        {
                            HookProperty = hook
                        };
                    }
                }
            }
        }

        internal static async Task<T> RunResultHooks<T>(this ICrudRequest request,
            ICrudRequestConfig config,
            T result,
            CancellationToken ct)
        {
            var hooks = config.GetResultHooks();

            foreach (var hook in hooks)
            {
                try
                {
                    result = (T)await hook.Run(request, result, ct).Configure();
                    ct.ThrowIfCancellationRequested();
                }
                catch (Exception e) when (IsHookFailure(e))
                {
                    throw new CrudHookFailedException(HookFailureReason("result"), e)
                    {
                        HookProperty = hook
                    };
                }
            }

            return result;
        }

        internal static async Task<T[]> RunResultHooks<T>(this ICrudRequest request,
            ICrudRequestConfig config,
            T[] results,
            CancellationToken ct)
        {
            var hooks = config.GetResultHooks();

            foreach (var hook in hooks)
            {
                for (var i = 0; i < results.Length; ++i)
                {
                    try
                    {
                        results[i] = (T)await hook.Run(request, results[i], ct).Configure();
                        ct.ThrowIfCancellationRequested();
                    }
                    catch (Exception e) when (IsHookFailure(e))
                    {
                        throw new CrudHookFailedException(HookFailureReason("result"), e)
                        {
                            HookProperty = hook
                        };
                    }
                }
            }

            return results;
        }

        internal static async Task<TEntity> CreateEntity<TEntity>(this ICrudRequest request,
            ICrudRequestConfig config,
            object item,
            CancellationToken token)
            where TEntity : class
        {
            var creator = config.GetCreatorFor<TEntity>();
            var entity = await creator(request, item, token).Configure();

            token.ThrowIfCancellationRequested();

            return entity;
        }

        internal static async Task<TEntity[]> CreateEntities<TEntity>(this IBulkRequest request,
            ICrudRequestConfig config,
            IEnumerable<object> items,
            CancellationToken token)
            where TEntity : class
        {
            var creator = config.GetCreatorFor<TEntity>();
            var entities = await Task.WhenAll(items.Select(x => creator(request, x, token))).Configure();

            token.ThrowIfCancellationRequested();

            return entities;
        }

        internal static async Task<TEntity> UpdateEntity<TEntity>(this ICrudRequest request,
            ICrudRequestConfig config,
            object item,
            TEntity entity,
            CancellationToken token)
            where TEntity : class
        {
            var updator = config.GetUpdatorFor<TEntity>();
            entity = await updator(request, item, entity, token).Configure();

            token.ThrowIfCancellationRequested();

            return entity;
        }

        internal static async Task<TEntity[]> UpdateEntities<TEntity>(this IBulkRequest request,
            ICrudRequestConfig config,
            IEnumerable<Tuple<object, TEntity>> items,
            CancellationToken token)
            where TEntity : class
        {
            var updator = config.GetUpdatorFor<TEntity>();
            var entities = await Task.WhenAll(items.Select(x => updator(request, x.Item1, x.Item2, token))).Configure();

            token.ThrowIfCancellationRequested();

            return entities;
        }
    }
}

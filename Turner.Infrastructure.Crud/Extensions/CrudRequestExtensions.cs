using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
            List<IBoxedRequestHook> hooks,
            CancellationToken ct)
        {
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

        internal static async Task<object[]> RunItemHooks(this IBulkRequest request,
            List<IBoxedItemHook> hooks,
            object[] items,
            CancellationToken ct)
        {
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

        internal static async Task RunEntityHooks(this ICrudRequest request,
            List<IBoxedEntityHook> hooks,
            object entity,
            CancellationToken ct)
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

        internal static async Task RunEntityHooks(this ICrudRequest request,
            List<IBoxedEntityHook> hooks,
            IEnumerable<object> entities,
            CancellationToken ct)
        {
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
            List<IBoxedResultHook> hooks,
            T result,
            CancellationToken ct)
        {
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

        internal static async Task<List<T>> RunResultHooks<T>(this ICrudRequest request,
            List<IBoxedResultHook> hooks,
            List<T> results,
            CancellationToken ct)
        {
            foreach (var hook in hooks)
            {
                for (var i = 0; i < results.Count; ++i)
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
    }
}

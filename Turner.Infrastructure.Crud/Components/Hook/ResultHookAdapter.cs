using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Requests;

namespace Turner.Infrastructure.Crud
{
    internal static class ResultHookAdapter
    {
        internal static async Task<T> Adapt<T>(IBoxedResultHook hook,
            object request,
            T result,
            CancellationToken ct)
        {
            if (typeof(IResultCollection<>).MakeGenericType(hook.ResultType).IsAssignableFrom(typeof(T)))
            {
                var adaptMethod = typeof(ResultHookAdapter)
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                    .Single(x => x.Name == "AdaptAsCollection");

                await (Task)adaptMethod
                    .MakeGenericMethod(hook.ResultType)
                    .Invoke(null, new object[] { hook, request, result, ct });

                return result;
            }

            throw new BadCrudConfigurationException($"Failed to adapt hook result type {typeof(T)}");
        }

        internal static async Task AdaptAsCollection<T>(IBoxedResultHook hook,
            object request,
            IResultCollection<T> items,
            CancellationToken ct)
        {
            var result = new List<T>(items.Items.Count);

            foreach (var item in items.Items)
                result.Add((T)await hook.Run(request, item, ct));

            items.Items = result;
        }
    }
}

using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud
{
    internal static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable Configure(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable<TResult> Configure<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(false);
        }
    }
}

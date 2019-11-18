using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud
{
    public interface IRequestHook<in TRequest>
    {
        Task Run(TRequest request, CancellationToken token = default(CancellationToken));
    }

    public interface IEntityHook<in TRequest, in TEntity>
        where TEntity : class
    {
        Task Run(TRequest request, TEntity entity, CancellationToken token = default(CancellationToken));
    }

    public interface IItemHook<in TRequest, TItem>
    {
        Task<TItem> Run(TRequest request, TItem item, CancellationToken token = default(CancellationToken));
    }

    public interface IResultHook<in TRequest, TResult>
    {
        Task<TResult> Run(TRequest request, TResult result, CancellationToken token = default(CancellationToken));
    }
}

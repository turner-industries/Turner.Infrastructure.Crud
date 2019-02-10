using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud
{
    // TODO: Add defaults to the cts
    public interface IRequestHook<TRequest>
    {
        Task Run(TRequest request, CancellationToken token);
    }

    public interface IEntityHook<TRequest, TEntity>
        where TEntity : class
    {
        Task Run(TRequest request, TEntity entity, CancellationToken token);
    }

    public interface IItemHook<TRequest, TItem>
    {
        Task<TItem> Run(TRequest request, TItem item, CancellationToken token);
    }

    public interface IResultHook<TRequest, TResult>
    {
        Task<TResult> Run(TRequest request, TResult result, CancellationToken token);
    }
}

using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud
{
    public interface IRequestHook<TRequest>
    {
        Task Run(TRequest request);
    }

    public interface IEntityHook<TRequest, TEntity>
        where TEntity : class
    {
        Task Run(TRequest request, TEntity entity);
    }

    public interface IItemHook<TRequest, TItem>
    {
        Task Run(TRequest request, TItem item);
    }
}

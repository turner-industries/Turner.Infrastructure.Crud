using System.Linq;

namespace Turner.Infrastructure.Crud
{
    public interface IFilter
    {
        IQueryable<T> Filter<TRequest, T>(TRequest request, IQueryable<T> queryable);
    }
}

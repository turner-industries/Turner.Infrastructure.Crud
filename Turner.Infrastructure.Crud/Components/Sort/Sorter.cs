using System.Linq;

namespace Turner.Infrastructure.Crud
{
    public enum SortDirection
    {
        Ascending = 0,
        Descending = 1,

        Default = Ascending
    }

    public interface ISorter
    {
        IOrderedQueryable<T> Sort<TRequest, T>(TRequest request, IQueryable<T> queryable);
    }
}

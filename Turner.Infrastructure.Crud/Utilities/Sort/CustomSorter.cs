using System;
using System.Linq;

namespace Turner.Infrastructure.Crud
{
    public class CustomSorter : ISorter
    {
        private Func<object, IQueryable, IOrderedQueryable> _customSort;

        internal void SetSort<TRequest, T>(Func<TRequest, IQueryable<T>, IOrderedQueryable<T>> customSort)
        {
            _customSort = (request, queryable) =>
                customSort((TRequest) request, (IQueryable<T>) queryable);
        }

        public IOrderedQueryable<T> Sort<TRequest, T>(TRequest request, IQueryable<T> queryable)
        {
            return _customSort(request, queryable) as IOrderedQueryable<T>;
        }
    }
}

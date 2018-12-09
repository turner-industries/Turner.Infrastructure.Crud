using System;
using System.Linq;

namespace Turner.Infrastructure.Crud
{
    public class CustomFilter : IFilter
    {
        private Func<object, IQueryable, IQueryable> _customFilter;

        internal void SetFilter<TRequest, T>(Func<TRequest, IQueryable<T>, IQueryable<T>> customFilter)
        {
            _customFilter = (request, queryable) =>
                customFilter((TRequest) request, (IQueryable<T>) queryable);
        }

        public IQueryable<T> Filter<TRequest, T>(TRequest request, IQueryable<T> queryable)
        {
            return _customFilter(request, queryable).Cast<T>();
        }
    }
}

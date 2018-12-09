using System;
using System.Linq;

namespace Turner.Infrastructure.Crud
{
    public class BasicFilter : IFilter
    {
        private Func<object, IQueryable, IQueryable> _filter;

        internal void SetFilter<TRequest, T>(Func<TRequest, IQueryable<T>, IQueryable<T>> filter,
            Func<TRequest, bool> predicate = null)
        {
            if (predicate == null)
            {
                _filter = (request, queryable) => filter((TRequest) request, (IQueryable<T>) queryable);
            }
            else
            {
                _filter = (request, queryable) => predicate((TRequest) request)
                    ? filter((TRequest) request, (IQueryable<T>) queryable)
                    : (IQueryable<T>) queryable;
            }
        }

        public IQueryable<T> Filter<TRequest, T>(TRequest request, IQueryable<T> queryable)
        {
            return _filter(request, queryable).Cast<T>();
        }
    }
}

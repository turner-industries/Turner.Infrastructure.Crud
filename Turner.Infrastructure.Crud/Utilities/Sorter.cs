using System;
using System.Collections.Generic;
using System.Linq;

namespace Turner.Infrastructure.Crud
{
    public interface ISorter
    {
        IOrderedQueryable<T> Sort<TRequest, T>(TRequest request, IQueryable<T> queryable);
    }
    
    public class SimpleSorter : ISorter
    {
        private readonly ISortOperation _sort;
        
        public SimpleSorter(ISortOperation sort)
        {
            _sort = sort;
        }

        public Func<object, bool> Predicate { get; set; }

        public IOrderedQueryable<T> Sort<TRequest, T>(TRequest request, IQueryable<T> queryable)
        {
            if (Predicate != null)
                return Predicate(request) ? _sort.Apply(queryable) : null;
            else
                return _sort.Apply(queryable);
        }
    }

    public class SwitchSorter<TValue> : ISorter
    {
        private readonly Dictionary<TValue, ISortOperation> _sorts
            = new Dictionary<TValue, ISortOperation>();

        private readonly ISortOperation _defaultSort;
        private readonly Func<object, TValue> _getSwitchValue;
        
        public SwitchSorter(Func<object, TValue> getSwitchValue, ISortOperation defaultSort)
        {
            _getSwitchValue = getSwitchValue;
            _defaultSort = defaultSort;
        }

        public void Case(TValue value, ISortOperation sort)
        {
            _sorts[value] = sort;
        }

        public IOrderedQueryable<T> Sort<TRequest, T>(TRequest request, IQueryable<T> queryable)
        {
            if (_sorts.TryGetValue(_getSwitchValue(request), out var sort))
                return sort.Apply(queryable);
            
            if (_defaultSort != null)
                return _defaultSort.Apply(queryable);

            return null;
        }
    }
}

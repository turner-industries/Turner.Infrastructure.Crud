using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud
{
    public class BasicSortOperation
    {
        private Func<IQueryable, IOrderedQueryable> _firstSortFunc;
        private readonly List<Func<IOrderedQueryable, IOrderedQueryable>> _restSortFuncs
            = new List<Func<IOrderedQueryable, IOrderedQueryable>>();

        private readonly Func<object, bool> _predicate;

        public BasicSortOperation(Func<object, bool> predicate)
        {
            _predicate = predicate;
        }

        internal void AddSort<T, TKey>(
            Expression<Func<T, TKey>> clause, 
            SortDirection direction)
        {
            if (_firstSortFunc == null)
            {
                _firstSortFunc = direction == SortDirection.Ascending
                    ? (Func<IQueryable, IOrderedQueryable>)
                        (queryable => ((IQueryable<T>) queryable).OrderBy(clause))
                    : queryable => ((IQueryable<T>) queryable).OrderByDescending(clause);
            }
            else
            {
                _restSortFuncs.Add(direction == SortDirection.Ascending
                    ? (Func<IOrderedQueryable, IOrderedQueryable>)
                        (queryable => ((IOrderedQueryable<T>) queryable).ThenBy(clause))
                    : queryable => ((IOrderedQueryable<T>) queryable).ThenByDescending(clause));
            }
        }

        public IOrderedQueryable<T> Sort<TRequest, T>(TRequest request, IQueryable<T> queryable)
        {
            if (_firstSortFunc == null ||
                (_predicate != null && !_predicate(request)))
                return null;
            
            var result = _firstSortFunc(queryable) as IOrderedQueryable<T>;
            foreach (var sortFunc in _restSortFuncs)
                result = sortFunc(result) as IOrderedQueryable<T>;

            return result;
        }
    }

    public class BasicSorter : ISorter
    {
        private readonly List<BasicSortOperation> _operations
            = new List<BasicSortOperation>();

        public BasicSorter(List<BasicSortOperation> operations)
        {
            _operations = operations;
        }
        
        public IOrderedQueryable<T> Sort<TRequest, T>(TRequest request, IQueryable<T> queryable)
        {
            foreach (var operation in _operations)
            {
                var result = operation.Sort(request, queryable);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}

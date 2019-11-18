using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud
{
    public class BasicSortOperation<TRequest, TEntity>
        where TEntity : class
    {
        private Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> _firstSortFunc;
        private readonly List<Func<IOrderedQueryable<TEntity>, IOrderedQueryable<TEntity>>> _restSortFuncs
            = new List<Func<IOrderedQueryable<TEntity>, IOrderedQueryable<TEntity>>>();

        private readonly Func<TRequest, bool> _predicate;

        public BasicSortOperation(Func<TRequest, bool> predicate)
        {
            _predicate = predicate;
        }

        internal void AddSort<TKey>(
            Expression<Func<TEntity, TKey>> clause, 
            SortDirection direction)
        {
            if (_firstSortFunc == null)
            {
                _firstSortFunc = direction == SortDirection.Ascending
                    ? (Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>)
                        (queryable => queryable.OrderBy(clause))
                    : queryable => queryable.OrderByDescending(clause);
            }
            else
            {
                _restSortFuncs.Add(direction == SortDirection.Ascending
                    ? (Func<IOrderedQueryable<TEntity>, IOrderedQueryable<TEntity>>)
                        (queryable => queryable.ThenBy(clause))
                    : queryable => queryable.ThenByDescending(clause));
            }
        }

        public IOrderedQueryable<TEntity> Sort(TRequest request, IQueryable<TEntity> queryable)
        {
            if (_firstSortFunc == null || (_predicate != null && !_predicate(request)))
                return null;
            
            var result = _firstSortFunc(queryable);
            foreach (var sortFunc in _restSortFuncs)
                result = sortFunc(result);

            return result;
        }
    }

    public class BasicSorter<TRequest, TEntity> : ISorter<TRequest, TEntity>
        where TEntity : class
    {
        private readonly List<BasicSortOperation<TRequest, TEntity>> _operations;

        public BasicSorter(List<BasicSortOperation<TRequest, TEntity>> operations)
        {
            _operations = operations;
        }
        
        public IOrderedQueryable<TEntity> Sort(TRequest request, IQueryable<TEntity> queryable)
        {
            foreach (var operation in _operations)
            {
                var result = operation.Sort(request, queryable);
                if (result != null)
                    return result;
            }
            
            return queryable as IOrderedQueryable<TEntity>;
        }
    }
}

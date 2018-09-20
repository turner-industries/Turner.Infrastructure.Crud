using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud
{
    public interface ISortOperation
    {
        IOrderedQueryable<T> Apply<T>(IQueryable<T> queryable);
    }
    
    public class SortOperation : ISortOperation
    {
        private readonly List<Func<IOrderedQueryable, IOrderedQueryable>> _secondarySortFuncs
            = new List<Func<IOrderedQueryable, IOrderedQueryable>>();

        private Func<IQueryable, IOrderedQueryable> _primarySortFunc;
        
        public SortOperation SetPrimary<T, TKey>(
            Expression<Func<T, TKey>> primaryClause,
            bool ascending)
        {
            _primarySortFunc = ascending
                ? (Func<IQueryable, IOrderedQueryable>)
                    (queryable => ((IQueryable<T>) queryable).OrderBy(primaryClause))
                : queryable => ((IQueryable<T>) queryable).OrderByDescending(primaryClause);
            
            return this;
        }
        
        public SortOperation AddSecondary<T, TKey>(
            Expression<Func<T, TKey>> secondaryClause,
            bool ascending)
        {
            _secondarySortFuncs.Add(ascending
                ? (Func<IOrderedQueryable, IOrderedQueryable>)
                    (queryable => ((IOrderedQueryable<T>) queryable).ThenBy(secondaryClause))
                : queryable => ((IOrderedQueryable<T>) queryable).ThenByDescending(secondaryClause));

            return this;
        }
        
        public IOrderedQueryable<T> Apply<T>(IQueryable<T> queryable)
        {
            var result = _primarySortFunc(queryable);

            foreach (var sortFunc in _secondarySortFuncs)
                result = sortFunc(result);

            return result as IOrderedQueryable<T>;
        }
    }
}

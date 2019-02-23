using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud
{
    public class TableSorter<TRequest, TEntity, TControl> : ISorter<TRequest, TEntity>
        where TEntity : class
    {
        private readonly List<TableSortControl> _controls = new List<TableSortControl>();
        private readonly Dictionary<TControl, TableSortOperation> _columns =
            new Dictionary<TControl, TableSortOperation>();

        internal void AddControl(Func<TRequest, TControl> getControl, Func<TRequest, SortDirection> getSortDirection)
        {
            var control = new TableSortControl
            {
                Control = getControl,
                Direction = getSortDirection
            };

            _controls.Add(control);
        }
        
        internal void AddColumn<TKey>(TControl value, Expression<Func<TEntity, TKey>> column)
        {
            var sortExpression = new TableSortOperation
            {
                CreateSort = (isPrimary, direction) =>
                {
                    if (isPrimary)
                    {
                        return direction == SortDirection.Ascending
                            ? (Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>)
                                (queryable => queryable.OrderBy(column))
                            : queryable => queryable.OrderByDescending(column);
                    }
                    else
                    {
                        return direction == SortDirection.Ascending
                            ? (Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>)
                                (queryable => ((IOrderedQueryable<TEntity>)queryable).ThenBy(column))
                            : (queryable => ((IOrderedQueryable<TEntity>)queryable).ThenByDescending(column));
                    }
                }
            };

            _columns[value] = sortExpression;
        }

        public IOrderedQueryable<TEntity> Sort(TRequest request, IQueryable<TEntity> queryable)
        {
            if (_controls.Count == 0)
                return null;

            var value = _controls[0].Control(request);
            var direction = _controls[0].Direction(request);

            if (value == null || !_columns.TryGetValue(value, out var sortOperation))
                return null;

            var sort = sortOperation.CreateSort(true, direction);
            var result = sort(queryable);

            foreach (var ctrl in _controls.Skip(1))
            {
                value = ctrl.Control(request);
                direction = ctrl.Direction(request);

                if (value == null || !_columns.TryGetValue(value, out sortOperation))
                    continue;

                sort = sortOperation.CreateSort(false, direction);
                result = sort(result);
            }
            
            return result;
        }
        
        private class TableSortControl
        {
            public Func<TRequest, TControl> Control { get; set; }

            public Func<TRequest, SortDirection> Direction { get; set; }
        }

        private class TableSortOperation
        {
            public Func<bool, SortDirection, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>> CreateSort { get; set; }
        }
    }
}

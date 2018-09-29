using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Turner.Infrastructure.Crud
{
    public class TableSorter<TControl> : ISorter
    {
        private readonly List<TableSortControl> _controls = new List<TableSortControl>();
        private readonly Dictionary<TControl, TableSortOperation> _columns =
            new Dictionary<TControl, TableSortOperation>();

        internal void AddControl<TRequest>(Func<TRequest, TControl> getControl, Func<TRequest, SortDirection> getSortDirection)
        {
            var control = new TableSortControl
            {
                Control = o => getControl((TRequest) o),
                Direction = o => getSortDirection((TRequest) o)
            };

            _controls.Add(control);
        }
        
        internal void AddColumn<T, TKey>(TControl value, Expression<Func<T, TKey>> column)
        {
            var sortExpression = new TableSortOperation
            {
                CreateSort = (isPrimary, direction) =>
                {
                    if (isPrimary)
                    {
                        return direction == SortDirection.Ascending
                            ? (Func<IQueryable, IOrderedQueryable>)
                                (q => ((IQueryable<T>) q).OrderBy(column))
                            : (q => ((IQueryable<T>) q).OrderByDescending(column));
                    }
                    else
                    {
                        return direction == SortDirection.Ascending
                            ? (Func<IQueryable, IOrderedQueryable>)
                                (q => ((IOrderedQueryable<T>) q).ThenBy(column))
                            : (q => ((IOrderedQueryable<T>) q).ThenByDescending(column));
                    }
                }
            };

            _columns[value] = sortExpression;
        }

        public IOrderedQueryable<T> Sort<TRequest, T>(TRequest request, IQueryable<T> queryable)
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
            
            return result as IOrderedQueryable<T>;
        }
        
        private class TableSortControl
        {
            public Func<object, TControl> Control { get; set; }
            public Func<object, SortDirection> Direction { get; set; }
        }

        private class TableSortOperation
        {
            public Func<bool, SortDirection, Func<IQueryable, IOrderedQueryable>> CreateSort { get; set; }
        }
    }
}

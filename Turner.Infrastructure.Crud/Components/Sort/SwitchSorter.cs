using System;
using System.Collections.Generic;
using System.Linq;

namespace Turner.Infrastructure.Crud
{
    public class SwitchSortOperation<TRequest, TEntity> : BasicSortOperation<TRequest, TEntity>
        where TEntity : class
    {
        public SwitchSortOperation()
            : base(null)
        {
        }
    }

    public class SwitchSorter<TRequest, TEntity, TValue> : ISorter<TRequest, TEntity>
        where TEntity : class
    {
        private readonly Dictionary<TValue, SwitchSortOperation<TRequest, TEntity>> _cases
            = new Dictionary<TValue, SwitchSortOperation<TRequest, TEntity>>();

        private readonly Func<TRequest, TValue> _getSwitchValue;

        private SwitchSortOperation<TRequest, TEntity> _default;

        public SwitchSorter(Func<TRequest, TValue> getSwitchValue)
        {
            _getSwitchValue = getSwitchValue;
        }

        internal void Case(TValue value, SwitchSortOperation<TRequest, TEntity> operation)
        {
            _cases[value] = operation;
        }

        internal void Default(SwitchSortOperation<TRequest, TEntity> operation)
        {
            _default = operation;
        }

        public IOrderedQueryable<TEntity> Sort(TRequest request, IQueryable<TEntity> queryable)
        {
            var switchValue = _getSwitchValue(request);
            
            if (switchValue != null && _cases.TryGetValue(switchValue, out var operation))
                return operation.Sort(request, queryable);

            if (_default != null)
                return _default.Sort(request, queryable);

            return null;
        }
    }
}

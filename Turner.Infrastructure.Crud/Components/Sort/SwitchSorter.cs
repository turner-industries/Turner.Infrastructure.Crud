using System;
using System.Collections.Generic;
using System.Linq;

namespace Turner.Infrastructure.Crud
{
    public class SwitchSortOperation : BasicSortOperation
    {
        public SwitchSortOperation()
            : base(null)
        {
        }
    }

    public class SwitchSorter<TValue> : ISorter
    {
        private readonly Dictionary<TValue, SwitchSortOperation> _cases
            = new Dictionary<TValue, SwitchSortOperation>();

        private readonly Func<object, TValue> _getSwitchValue;

        private SwitchSortOperation _default;

        public SwitchSorter(Func<object, TValue> getSwitchValue)
        {
            _getSwitchValue = getSwitchValue;
        }

        internal void Case(TValue value, SwitchSortOperation operation)
        {
            _cases[value] = operation;
        }

        internal void Default(SwitchSortOperation operation)
        {
            _default = operation;
        }

        public IOrderedQueryable<T> Sort<TRequest, T>(TRequest request, IQueryable<T> queryable)
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

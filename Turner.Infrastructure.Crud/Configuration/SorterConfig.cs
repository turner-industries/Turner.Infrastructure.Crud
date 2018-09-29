using System;
using System.Collections.Generic;

namespace Turner.Infrastructure.Crud.Configuration
{
    public enum SorterType
    {
        GetAll
    }

    public class SorterConfig
    {
        private readonly Dictionary<SorterType, SorterConfigSet> _sorters
            = new Dictionary<SorterType, SorterConfigSet>();

        public SorterConfig()
        {
            foreach (var type in (SorterType[]) Enum.GetValues(typeof(SorterType)))
                _sorters[type] = new SorterConfigSet();
        }

        public SorterConfigSet this[SorterType type] => _sorters[type];

        internal void Set(SorterType type, Type tEntity, ISorter sorter)
        {
            _sorters[type].SetSorterFor(tEntity, sorter);
        }
    }

    public class SorterConfigSet
    {
        private readonly Dictionary<Type, ISorter> _entitySorters
            = new Dictionary<Type, ISorter>();

        public ISorter GetSorterFor(Type tEntity)
        {
            foreach (var type in tEntity.BuildTypeHierarchyUp())
            {
                if (_entitySorters.TryGetValue(type, out var sorters))
                    return sorters;
            }

            return null;
        }

        internal void SetSorterFor(Type tEntity, ISorter sorter)
        {
            _entitySorters[tEntity] = sorter;
        }
    }
}

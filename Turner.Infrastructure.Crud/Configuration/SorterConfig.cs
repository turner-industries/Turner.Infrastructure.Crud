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

        internal void Set(SorterType type, Type tEntity, List<ISorter> sorters)
        {
            _sorters[type].SetSortersFor(tEntity, sorters);
        }
    }

    public class SorterConfigSet
    {
        private readonly Dictionary<Type, List<ISorter>> _entitySorters
            = new Dictionary<Type, List<ISorter>>();

        public List<ISorter> GetSortersFor(Type tEntity)
        {
            foreach (var type in tEntity.BuildTypeHierarchyUp())
            {
                if (_entitySorters.TryGetValue(type, out var sorters))
                    return sorters;
            }

            return null;
        }

        internal void SetSortersFor(Type tEntity, List<ISorter> sorters)
        {
            _entitySorters[tEntity] = sorters;
        }
    }
}

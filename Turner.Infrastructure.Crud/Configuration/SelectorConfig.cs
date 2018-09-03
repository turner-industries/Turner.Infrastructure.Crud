using System;
using System.Collections.Generic;
using System.Linq;

namespace Turner.Infrastructure.Crud.Configuration
{
    public enum SelectorType
    {
        Get,
        Update,
        Delete
    }

    public class SelectorConfig
    {
        private readonly Dictionary<SelectorType, SelectorConfigSet> _selectors
            = new Dictionary<SelectorType, SelectorConfigSet>();

        public SelectorConfig()
        {
            foreach (var type in (SelectorType[]) Enum.GetValues(typeof(SelectorType)))
                _selectors[type] = new SelectorConfigSet();
        }

        public SelectorConfigSet this[SelectorType type] => _selectors[type];

        internal void Set(SelectorType type, Type tEntity, ISelector selector)
        {
            _selectors[type].SetSelectorFor(tEntity, selector);
        }
    }

    public class SelectorConfigSet
    {
        private readonly Dictionary<Type, ISelector> _entitySelectors
            = new Dictionary<Type, ISelector>();

        public ISelector FindSelectorFor(Type tEntity)
        {
            foreach (var type in tEntity.BuildTypeHierarchyUp())
            {
                if (_entitySelectors.TryGetValue(type, out var selector))
                    return selector;
            }

            return null;
        }

        internal void SetSelectorFor(Type tEntity, ISelector selector)
        {
            _entitySelectors[tEntity] = selector;
        }
    }
}

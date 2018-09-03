using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud.Configuration
{
    public enum ActionType
    {
        Create,
        Update,
        Delete
    }

    public class ActionConfig
    {
        private readonly Dictionary<ActionType, ActionSet> _actions
            = new Dictionary<ActionType, ActionSet>();

        public ActionConfig()
        {
            foreach (var type in (ActionType[]) Enum.GetValues(typeof(ActionType)))
                _actions[type] = new ActionSet();
        }

        public ActionSet this[ActionType type] => _actions[type];
    }

    public class ActionSet
    {
        private readonly ActionList _requestPreActions 
            = new ActionList();

        private readonly Dictionary<Type, ActionList> _entityPreActions
            = new Dictionary<Type, ActionList>();

        private readonly Dictionary<Type, ActionList> _entityPostActions
            = new Dictionary<Type, ActionList>();

        internal void AddPreActions(ActionList actions)
        {
            _requestPreActions.InsertRange(0, actions);
        }

        internal void SetPreActionsFor(Type tEntity, ActionList actions)
        {
            _entityPreActions[tEntity] = actions;
        }

        internal void SetPostActionsFor(Type tEntity, ActionList actions)
        {
            _entityPostActions[tEntity] = actions;
        }

        internal async Task RunPreActionsFor(Type tEntity, object request)
        {
            foreach (var requestAction in _requestPreActions)
                await requestAction(request).Configure();
            
            foreach (var type in BuildEntityHierarchy(tEntity))
            {
                if (_entityPreActions.TryGetValue(type, out var actions))
                {
                    foreach (var action in actions)
                        await action(request).Configure();
                }
            }
        }

        internal async Task RunPostActionsFor(Type tEntity, object entity)
        {
            foreach (var type in BuildEntityHierarchy(tEntity))
            {
                if (_entityPostActions.TryGetValue(type, out var actions))
                {
                    foreach (var action in actions)
                        await action(entity).Configure();
                }
            }
        }

        private IEnumerable<Type> BuildEntityHierarchy(Type tEntity)
        {
            var entityParents = new[] { tEntity.BaseType }
                .Concat(tEntity.GetInterfaces())
                .Where(x => x != null);

            foreach (var parent in entityParents)
                foreach (var item in BuildEntityHierarchy(parent))
                    yield return item;

            yield return tEntity;
        }
    }

    public class ActionList : List<Func<object, Task>>
    {
        public ActionList() : base() { }
        public ActionList(int capacity) : base(capacity) { }
        public ActionList(IEnumerable<Func<object, Task>> actions) : base(actions) { }
    }
}

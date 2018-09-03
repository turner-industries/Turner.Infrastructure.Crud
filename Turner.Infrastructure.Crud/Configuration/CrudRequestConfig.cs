using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Errors;

namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudRequestConfig
    {
        bool FailedToFindInGetIsError { get; }
        bool FailedToFindInUpdateIsError { get; }
        bool FailedToFindInDeleteIsError { get; }

        ISelector GetSelector<TEntity>()
            where TEntity : class;

        ISelector UpdateSelector<TEntity>()
            where TEntity : class;

        ISelector DeleteSelector<TEntity>()
            where TEntity : class;

        Task<TEntity> CreateEntity<TEntity>(object request)
            where TEntity : class;

        Task UpdateEntity<TEntity>(object request, TEntity entity)
            where TEntity : class;

        TEntity GetDefault<TEntity>()
            where TEntity : class;

        Task PreCreate<TEntity>(object request)
            where TEntity : class;

        Task PostCreate<TEntity>(TEntity entity)
            where TEntity : class;

        Task PreUpdate<TEntity>(object request)
            where TEntity : class;

        Task PostUpdate<TEntity>(TEntity entity)
            where TEntity : class;

        Task PreDelete<TEntity>(object request)
            where TEntity : class;

        Task PostDelete<TEntity>(TEntity entity)
            where TEntity : class;
    }

    public class CrudRequestConfig<TRequest>
        : ICrudRequestConfig
    {
        private readonly Dictionary<Type, ISelector> _entityGetSelectors
            = new Dictionary<Type, ISelector>();

        private readonly Dictionary<Type, ISelector> _entityUpdateSelectors
            = new Dictionary<Type, ISelector>();

        private readonly Dictionary<Type, ISelector> _entityDeleteSelectors
            = new Dictionary<Type, ISelector>();

        private readonly Dictionary<Type, Func<object, Task<object>>> _entityCreators
            = new Dictionary<Type, Func<object, Task<object>>>();

        private readonly Dictionary<Type, Func<object, object, Task>> _entityUpdators
            = new Dictionary<Type, Func<object, object, Task>>();

        private readonly Dictionary<Type, object> _defaultValues
            = new Dictionary<Type, object>();

        // TODO: move these into a utility class
        private List<Func<object, Task>> _preCreateActions
            = new List<Func<object, Task>>();

        private List<Func<object, Task>> _preUpdateActions
            = new List<Func<object, Task>>();

        private List<Func<object, Task>> _preDeleteActions
            = new List<Func<object, Task>>();

        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPreCreateActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPostCreateActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPreUpdateActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPostUpdateActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPreDeleteActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPostDeleteActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        public CrudRequestConfig()
        {
            FailedToFindInGetIsError = true;
            FailedToFindInUpdateIsError = true;
            FailedToFindInDeleteIsError = false;
        }

        internal void SetFailedToFindInGetIsError(bool isError)
        {
            FailedToFindInGetIsError = isError;
        }

        internal void SetFailedToFindInUpdateIsError(bool isError)
        {
            FailedToFindInUpdateIsError = isError;
        }

        internal void SetFailedToFindInDeleteIsError(bool isError)
        {
            FailedToFindInDeleteIsError = isError;
        }

        internal void SetEntitySelectorForGet<TEntity>(ISelector selector)
            where TEntity : class
        {
            _entityGetSelectors[typeof(TEntity)] = selector;
        }

        internal void SetEntitySelectorForUpdate<TEntity>(ISelector selector)
            where TEntity : class
        {
            _entityUpdateSelectors[typeof(TEntity)] = selector;
        }

        internal void SetEntitySelectorForDelete<TEntity>(ISelector selector)
            where TEntity : class
        {
            _entityDeleteSelectors[typeof(TEntity)] = selector;
        }

        internal void SetEntityCreator<TEntity>(
            Func<object, Task<TEntity>> creator)
            where TEntity : class
        {
            _entityCreators[typeof(TEntity)] = async request => await creator(request).Configure();
        }

        internal void SetEntityUpdator<TEntity>(
            Func<object, TEntity, Task> updator)
            where TEntity : class
        {
            _entityUpdators[typeof(TEntity)] = (request, entity) => updator(request, (TEntity) entity);
        }

        internal void SetDefault<TEntity>(
            TEntity defaultValue)
            where TEntity : class
        {
            _defaultValues[typeof(TEntity)] = defaultValue;
        }

        internal void AddPreCreateActions(
            List<Func<object, Task>> actions)
        {
            _preCreateActions.InsertRange(0, actions);
        }

        internal void AddPreUpdateActions(
            List<Func<object, Task>> actions)
        {
            _preUpdateActions.InsertRange(0, actions);
        }

        internal void AddPreDeleteActions(
            List<Func<object, Task>> actions)
        {
            _preDeleteActions.InsertRange(0, actions);
        }

        internal void SetPreCreateActions<TEntity>(
            List<Func<object, Task>> actions)
            where TEntity : class
        {
            _entityPreCreateActions[typeof(TEntity)] = actions;
        }

        internal void SetPostCreateActions<TEntity>(
            List<Func<object, Task>> actions)
            where TEntity : class
        {
            _entityPostCreateActions[typeof(TEntity)] = actions;
        }

        internal void SetPreUpdateActions<TEntity>(
            List<Func<object, Task>> actions)
            where TEntity : class
        {
            _entityPreUpdateActions[typeof(TEntity)] = actions;
        }

        internal void SetPostUpdateActions<TEntity>(
            List<Func<object, Task>> actions)
            where TEntity : class
        {
            _entityPostUpdateActions[typeof(TEntity)] = actions;
        }

        internal void SetPreDeleteActions<TEntity>(
            List<Func<object, Task>> actions)
            where TEntity : class
        {
            _entityPreDeleteActions[typeof(TEntity)] = actions;
        }

        internal void SetPostDeleteActions<TEntity>(
            List<Func<object, Task>> actions)
            where TEntity : class
        {
            _entityPostDeleteActions[typeof(TEntity)] = actions;
        }
        
        public bool FailedToFindInGetIsError { get; private set; }

        public bool FailedToFindInUpdateIsError { get; private set; }

        public bool FailedToFindInDeleteIsError { get; private set; }

        public ISelector GetSelector<TEntity>()
            where TEntity : class
        {
            return FindSelector<TEntity>(_entityGetSelectors);
        }

        public ISelector UpdateSelector<TEntity>()
            where TEntity : class
        {
            return FindSelector<TEntity>(_entityUpdateSelectors);
        }

        public ISelector DeleteSelector<TEntity>()
            where TEntity : class
        {
            return FindSelector<TEntity>(_entityDeleteSelectors);
        }

        public async Task<TEntity> CreateEntity<TEntity>(object request)
            where TEntity : class
        {
            if (!(request is TRequest))
            {
                var message = 
                    $"Unable to create an entity of type '{typeof(TEntity)}' " +
                    $"from a request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'."; 

                throw new BadCrudConfigurationException(message);
            }

            if (_entityCreators.TryGetValue(typeof(TEntity), out var creator))
                return (TEntity) await creator(request).Configure();
            
            return Mapper.Map<TEntity>(request);
        }

        public Task UpdateEntity<TEntity>(object request, TEntity entity)
            where TEntity : class
        {
            if (!(request is TRequest))
            {
                var message =
                    $"Unable to update an entity of type '{typeof(TEntity)}' " +
                    $"from a request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'.";

                throw new BadCrudConfigurationException(message);
            }

            if (_entityUpdators.TryGetValue(typeof(TEntity), out var updator))
                return updator(request, entity);
        
            Mapper.Map(request, entity);

            return Task.CompletedTask;
        }

        public TEntity GetDefault<TEntity>()
            where TEntity : class
        {
            if (_defaultValues.TryGetValue(typeof(TEntity), out var entity))
                return (TEntity) entity;

            return null;
        }

        public Task PreCreate<TEntity>(object request) 
            where TEntity : class
        {
            return PreActions<TEntity>(request, _preCreateActions, _entityPreCreateActions);
        }

        public Task PostCreate<TEntity>(TEntity entity) 
            where TEntity : class
        {
            return PostActions(entity, _entityPostCreateActions);
        }

        public Task PreUpdate<TEntity>(object request)
            where TEntity : class
        {
            return PreActions<TEntity>(request, _preUpdateActions, _entityPreUpdateActions);
        }

        public Task PostUpdate<TEntity>(TEntity entity)
            where TEntity : class
        {
            return PostActions(entity, _entityPostUpdateActions);
        }

        public Task PreDelete<TEntity>(object request)
            where TEntity : class
        {
            return PreActions<TEntity>(request, _preDeleteActions,_entityPreDeleteActions);
        }

        public Task PostDelete<TEntity>(TEntity entity)
            where TEntity : class
        {
            return PostActions(entity, _entityPostDeleteActions);
        }

        private async Task PreActions<TEntity>(object request, 
            IEnumerable<Func<object, Task>> requestActions, 
            IReadOnlyDictionary<Type, List<Func<object, Task>>> actionMap)
        {
            if (!(request is TRequest))
            {
                var message =
                    $"Unable to run PreUpdate actions on a request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'.";

                throw new BadCrudConfigurationException(message);
            }

            foreach (var requestAction in requestActions)
                await requestAction(request).Configure();

            var entities = new List<Type>();
            BuildEntityStack(typeof(TEntity), ref entities);

            foreach (var tEntity in entities)
            {
                if (actionMap.TryGetValue(tEntity, out var actions))
                {
                    foreach (var action in actions)
                        await action(request).Configure();
                }
            }
        }

        private async Task PostActions<TEntity>(TEntity entity, 
            IReadOnlyDictionary<Type, List<Func<object, Task>>> actionMap)
            where TEntity : class
        {
            var entities = new List<Type>();
            BuildEntityStack(typeof(TEntity), ref entities);

            foreach (var tEntity in entities)
            {
                if (actionMap.TryGetValue(tEntity, out var actions))
                {
                    foreach (var action in actions)
                        await action(entity).Configure();
                }
            }
        }

        private ISelector FindSelector<TEntity>(IReadOnlyDictionary<Type, ISelector> selectors)
            where TEntity : class
        {
            var entities = new List<Type>();
            BuildEntityQueue(typeof(TEntity), ref entities);

            foreach (var tEntity in entities)
            {
                if (selectors.TryGetValue(tEntity, out var selector))
                    return selector;
            }

            throw new BadCrudConfigurationException(
                $"No selector defined for entity '{typeof(TEntity)}' for request '{typeof(TRequest)}'.");
        }

        private void BuildEntityStack(Type tEntity, ref List<Type> entities)
        {
            var entityParents = new[] { tEntity.BaseType }
                .Concat(tEntity.GetInterfaces())
                .Where(x => x != null);

            foreach (var parent in entityParents)
                BuildEntityStack(parent, ref entities);

            entities.Add(tEntity);
        }

        private void BuildEntityQueue(Type tEntity, ref List<Type> entities)
        {
            entities.Add(tEntity);

            var entityParents = new[] { tEntity.BaseType }
                .Concat(tEntity.GetInterfaces())
                .Where(x => x != null);

            foreach (var parent in entityParents)
                BuildEntityStack(parent, ref entities);
        }
    }
}

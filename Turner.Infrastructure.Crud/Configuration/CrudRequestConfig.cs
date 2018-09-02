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

        ISelector GetSelector<TEntity>()
            where TEntity : class;

        ISelector UpdateSelector<TEntity>()
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
    }

    public class CrudRequestConfig<TRequest>
        : ICrudRequestConfig
    {
        private readonly Dictionary<Type, ISelector> _entityGetSelectors
            = new Dictionary<Type, ISelector>();

        private readonly Dictionary<Type, ISelector> _entityUpdateSelectors
            = new Dictionary<Type, ISelector>();

        private readonly Dictionary<Type, Func<object, Task<object>>> _entityCreators
            = new Dictionary<Type, Func<object, Task<object>>>();

        private readonly Dictionary<Type, Func<object, object, Task>> _entityUpdators
            = new Dictionary<Type, Func<object, object, Task>>();

        private readonly Dictionary<Type, object> _defaultValues
            = new Dictionary<Type, object>();

        // TODO: move these into a utility class
        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPreCreateActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPostCreateActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPreUpdateActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPostUpdateActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        public CrudRequestConfig()
        {
            FailedToFindInGetIsError = true;
            FailedToFindInUpdateIsError = true;
        }

        internal void SetFailedToFindInGetIsError(bool isError)
        {
            FailedToFindInGetIsError = isError;
        }

        internal void SetFailedToFindInUpdateIsError(bool isError)
        {
            FailedToFindInUpdateIsError = isError;
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

        internal void SetEntityCreator<TEntity>(
            Func<object, Task<TEntity>> creator)
            where TEntity : class
        {
            _entityCreators[typeof(TEntity)] = async request => await creator(request);
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

        public bool FailedToFindInGetIsError { get; private set; }

        public bool FailedToFindInUpdateIsError { get; private set; }

        public ISelector GetSelector<TEntity>()
            where TEntity : class
        {
            var entities = new List<Type>();
            BuildEntityQueue(typeof(TEntity), ref entities);

            foreach (var tEntity in entities)
            {
                if (_entityGetSelectors.TryGetValue(tEntity, out var selector))
                    return selector;
            }

            throw new BadCrudConfigurationException(
                $"No selector defined for entity '{typeof(TEntity)}' for request '{typeof(TRequest)}'.");
        }

        public ISelector UpdateSelector<TEntity>()
            where TEntity : class
        {
            var entities = new List<Type>();
            BuildEntityQueue(typeof(TEntity), ref entities);

            foreach (var tEntity in entities)
            {
                if (_entityUpdateSelectors.TryGetValue(tEntity, out var selector))
                    return selector;
            }

            throw new BadCrudConfigurationException(
                $"No selector defined for entity '{typeof(TEntity)}' for request '{typeof(TRequest)}'.");
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
                return (TEntity) await creator(request);
            
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

        public async Task PreCreate<TEntity>(object request) 
            where TEntity : class
        {
            if (!(request is TRequest))
            {
                var message =
                    $"Unable to run PreCreate actions on a request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'.";

                throw new BadCrudConfigurationException(message);
            }

            var entities = new List<Type>();
            BuildEntityStack(typeof(TEntity), ref entities);

            foreach (var tEntity in entities)
            {
                if (_entityPreCreateActions.TryGetValue(tEntity, out var actions))
                {
                    foreach (var action in actions)
                        await action(request);
                }
            }
        }

        public async Task PostCreate<TEntity>(TEntity entity) 
            where TEntity : class
        {
            var entities = new List<Type>();
            BuildEntityStack(typeof(TEntity), ref entities);

            foreach (var tEntity in entities)
            {
                if (_entityPostCreateActions.TryGetValue(tEntity, out var actions))
                {
                    foreach (var action in actions)
                        await action(entity);
                }
            }
        }

        public async Task PreUpdate<TEntity>(object request)
            where TEntity : class
        {
            if (!(request is TRequest))
            {
                var message =
                    $"Unable to run PreUpdate actions on a request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'.";

                throw new BadCrudConfigurationException(message);
            }

            var entities = new List<Type>();
            BuildEntityStack(typeof(TEntity), ref entities);

            foreach (var tEntity in entities)
            {
                if (_entityPreUpdateActions.TryGetValue(tEntity, out var actions))
                {
                    foreach (var action in actions)
                        await action(request);
                }
            }
        }

        public async Task PostUpdate<TEntity>(TEntity entity)
            where TEntity : class
        {
            var entities = new List<Type>();
            BuildEntityStack(typeof(TEntity), ref entities);

            foreach (var tEntity in entities)
            {
                if (_entityPostUpdateActions.TryGetValue(tEntity, out var actions))
                {
                    foreach (var action in actions)
                        await action(entity);
                }
            }
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

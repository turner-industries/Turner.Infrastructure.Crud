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
        bool FailedToFindIsError { get; }

        ISelector GetSelector<TEntity>()
            where TEntity : class;

        TEntity CreateEntity<TEntity>(object request)
            where TEntity : class;

        TEntity GetDefault<TEntity>()
            where TEntity : class;

        Task PreCreate<TEntity>(object request)
            where TEntity : class;

        Task PostCreate<TEntity>(TEntity entity)
            where TEntity : class;
    }

    public class CrudRequestConfig<TRequest>
        : ICrudRequestConfig
    {
        private readonly Dictionary<Type, ISelector> _entitySelectors
            = new Dictionary<Type, ISelector>();

        private readonly Dictionary<Type, Func<object, object>> _entityCreators
            = new Dictionary<Type, Func<object, object>>();

        private readonly Dictionary<Type, object> _defaultValues
            = new Dictionary<Type, object>();

        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPreCreateActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        private readonly Dictionary<Type, List<Func<object, Task>>> _entityPostCreateActions
            = new Dictionary<Type, List<Func<object, Task>>>();

        public CrudRequestConfig()
        {
            FailedToFindIsError = true;
        }

        internal void SetFailedToFindIsError(bool isError)
        {
            FailedToFindIsError = isError;
        }

        internal void SetEntitySelector<TEntity>(ISelector selector)
            where TEntity : class
        {
            _entitySelectors[typeof(TEntity)] = selector;
        }

        internal void SetEntityCreator<TEntity>(
            Func<object, TEntity> creator)
            where TEntity : class
        {
            _entityCreators[typeof(TEntity)] = request => creator(request);
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

        public bool FailedToFindIsError { get; private set; }

        public ISelector GetSelector<TEntity>()
            where TEntity : class
        {
            var entities = new List<Type>();
            BuildEntityQueue(typeof(TEntity), ref entities);

            foreach (var tEntity in entities)
            {
                if (_entitySelectors.TryGetValue(tEntity, out var selector))
                    return selector;
            }

            throw new BadCrudConfigurationException(
                $"No selector defined for entity '{typeof(TEntity)}'.");
        }
        
        public TEntity CreateEntity<TEntity>(object request)
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
                return (TEntity) creator(request);
            
            return Mapper.Map<TEntity>(request);
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

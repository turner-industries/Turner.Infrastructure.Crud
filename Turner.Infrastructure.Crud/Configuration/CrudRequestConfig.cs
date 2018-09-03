using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Errors;

namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudRequestConfig
    {
        ErrorConfig ErrorConfig { get; }

        ISelector GetSelectorFor<TEntity>(SelectorType type)
            where TEntity : class;
        
        Task RunPreActionsFor<TEntity>(ActionType type, object request)
            where TEntity : class;

        Task RunPostActionsFor<TEntity>(ActionType type, TEntity entity)
            where TEntity : class;

        Task<TEntity> CreateEntity<TEntity>(object request)
            where TEntity : class;

        Task UpdateEntity<TEntity>(object request, TEntity entity)
            where TEntity : class;

        TEntity GetDefault<TEntity>()
            where TEntity : class;
    }

    public class CrudRequestConfig<TRequest>
        : ICrudRequestConfig
    {
        private readonly SelectorConfig _selectors = new SelectorConfig();
        private readonly ActionConfig _actions = new ActionConfig();

        private readonly Dictionary<Type, Func<object, Task<object>>> _entityCreators
            = new Dictionary<Type, Func<object, Task<object>>>();

        private readonly Dictionary<Type, Func<object, object, Task>> _entityUpdators
            = new Dictionary<Type, Func<object, object, Task>>();

        private readonly Dictionary<Type, object> _defaultValues
            = new Dictionary<Type, object>();
        
        internal void SetEntitySelectorFor<TEntity>(SelectorType type, ISelector selector)
            where TEntity : class
        {
            _selectors.Set(type, typeof(TEntity), selector);
        }
        
        internal void AddPreActions(ActionType type, ActionList actions)
        {
            _actions[type].AddPreActions(actions);
        }
        
        internal void SetPreActionsFor<TEntity>(ActionType type, ActionList actions)
            where TEntity : class
        {
            _actions[type].SetPreActionsFor(typeof(TEntity), actions);
        }
        
        internal void SetPostActionsFor<TEntity>(ActionType type, ActionList actions)
            where TEntity : class
        {
            _actions[type].SetPostActionsFor(typeof(TEntity), actions);
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
            _entityUpdators[typeof(TEntity)] = (request, entity) => updator(request, (TEntity)entity);
        }

        internal void SetDefault<TEntity>(
            TEntity defaultValue)
            where TEntity : class
        {
            _defaultValues[typeof(TEntity)] = defaultValue;
        }

        public ErrorConfig ErrorConfig { get; private set; } = new ErrorConfig();

        public ISelector GetSelectorFor<TEntity>(SelectorType type)
            where TEntity : class
        {
            var selector = _selectors[type].FindSelectorFor(typeof(TEntity))
                ?? throw new BadCrudConfigurationException(
                    $"No selector defined for entity '{typeof(TEntity)}' " +
                    $"for request '{typeof(TRequest)}'.");

            return selector;
        }

        public Task RunPreActionsFor<TEntity>(ActionType type, object request)
            where TEntity : class
        {
            if (!(request is TRequest))
            {
                var message =
                    $"Unable to run {type.ToString()} actions on a request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'.";

                throw new BadCrudConfigurationException(message);
            }

            return _actions[type].RunPreActionsFor(typeof(TEntity), request);
        }

        public Task RunPostActionsFor<TEntity>(ActionType type, TEntity entity)
            where TEntity : class
        {
            return _actions[type].RunPostActionsFor(typeof(TEntity), entity);
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
    }
}

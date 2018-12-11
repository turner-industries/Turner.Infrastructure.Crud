using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Exceptions;

namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudRequestConfig
    {
        ErrorConfig ErrorConfig { get; }

        RequestOptions GetOptionsFor<TEntity>()
            where TEntity : class;

        ISelector GetSelectorFor<TEntity>()
            where TEntity : class;

        ISorter GetSorterFor<TEntity>()
            where TEntity : class;

        TEntity GetDefaultFor<TEntity>()
            where TEntity : class;

        IEnumerable<IFilter> GetFiltersFor<TEntity>();

        Task RunPreActionsFor<TEntity>(ActionType type, object request)
            where TEntity : class;

        Task RunPostActionsFor<TEntity>(ActionType type, object request, TEntity entity)
            where TEntity : class;

        Task<TEntity> CreateEntity<TEntity>(object request)
            where TEntity : class;

        Task<TEntity[]> CreateEntities<TEntity>(object request)
            where TEntity : class;

        Task UpdateEntity<TEntity>(object request, TEntity entity)
            where TEntity : class;
    }

    public class CrudRequestConfig<TRequest>
        : ICrudRequestConfig
    {
        private readonly ActionConfig _actions = new ActionConfig();
        private readonly RequestOptions _options = new RequestOptions();
        private readonly Dictionary<Type, CrudOptionsConfig> _entityOptionOverrides
            = new Dictionary<Type, CrudOptionsConfig>();

        private readonly Dictionary<Type, RequestOptions> _optionsCache
            = new Dictionary<Type, RequestOptions>();

        private readonly Dictionary<Type, ISorter> _entitySorters
            = new Dictionary<Type, ISorter>();

        private readonly Dictionary<Type, ISelector> _entitySelectors
            = new Dictionary<Type, ISelector>();

        private readonly Dictionary<Type, Func<object, Task<object>>> _entityCreators
            = new Dictionary<Type, Func<object, Task<object>>>();

        private readonly Dictionary<Type, Func<object, Task<object[]>>> _entitiesCreators
            = new Dictionary<Type, Func<object, Task<object[]>>>();

        private readonly Dictionary<Type, Func<object, object, Task>> _entityUpdators
            = new Dictionary<Type, Func<object, object, Task>>();

        private readonly Dictionary<Type, object> _defaultValues
            = new Dictionary<Type, object>();

        private readonly Dictionary<Type, List<IFilter>> _entityFilters
            = new Dictionary<Type, List<IFilter>>();
        
        public ErrorConfig ErrorConfig { get; private set; } = new ErrorConfig();

        public RequestOptions GetOptionsFor<TEntity>()
            where TEntity : class
        {
            if (_optionsCache.TryGetValue(typeof(TEntity), out var cachedOptions))
                return cachedOptions;

            var options = _options.Clone();
            OverrideOptions(options, typeof(TEntity));

            _optionsCache[typeof(TEntity)] = options;

            return options;
        }
        
        public Task RunPreActionsFor<TEntity>(ActionType type, object request)
            where TEntity : class
        {
            if (!(request is TRequest))
            {
                var message =
                    $"Unable to run {type.ToString()} pre actions on a request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'.";

                throw new BadCrudConfigurationException(message);
            }

            return _actions[type].RunPreActionsFor(typeof(TEntity), request);
        }

        public Task RunPostActionsFor<TEntity>(ActionType type, object request, TEntity entity)
            where TEntity : class
        {
            if (!(request is TRequest))
            {
                var message =
                    $"Unable to run {type.ToString()} post actions on a request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'.";

                throw new BadCrudConfigurationException(message);
            }

            return _actions[type].RunPostActionsFor(typeof(TEntity), request, entity);
        }

        public ISelector GetSelectorFor<TEntity>()
            where TEntity : class
        {
            foreach (var type in typeof(TEntity).BuildTypeHierarchyUp())
            {
                if (_entitySelectors.TryGetValue(type, out var selector))
                    return selector;
            }

            throw new BadCrudConfigurationException(
                $"No selector defined for entity '{typeof(TEntity)}' " +
                $"for request '{typeof(TRequest)}'.");
        }
        
        public ISorter GetSorterFor<TEntity>()
            where TEntity : class
        {
            foreach (var type in typeof(TEntity).BuildTypeHierarchyUp())
            {
                if (_entitySorters.TryGetValue(type, out var sorter))
                    return sorter;
            }

            return null;
        }

        public IEnumerable<IFilter> GetFiltersFor<TEntity>()
        {
            foreach (var type in typeof(TEntity).BuildTypeHierarchyDown())
            {
                if (_entityFilters.TryGetValue(type, out var filters))
                {
                    foreach (var filter in filters)
                        yield return filter;
                }
            }
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

        public async Task<TEntity[]> CreateEntities<TEntity>(object request)
            where TEntity : class
        {
            if (!(request is TRequest))
            {
                var message =
                    $"Unable to create entities of type '{typeof(TEntity)}' " +
                    $"from a request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'.";

                throw new BadCrudConfigurationException(message);
            }

            if (_entitiesCreators.TryGetValue(typeof(TEntity), out var creator))
                return (TEntity[]) await creator(request).Configure();

            return Mapper.Map<TEntity[]>(request);
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

        public TEntity GetDefaultFor<TEntity>()
            where TEntity : class
        {
            if (_defaultValues.TryGetValue(typeof(TEntity), out var entity))
                return (TEntity) entity;

            return null;
        }

        internal void SetOptions(CrudOptionsConfig options)
        {
            if (options != null)
                OverrideOptions(_options, options);
        }

        internal void SetOptionsFor<TEntity>(CrudOptionsConfig options)
        {
            _entityOptionOverrides[typeof(TEntity)] = options;
        }

        internal void AddPreActions(ActionType type, ActionList actions)
        {
            _actions[type].AddPreActions(actions);
        }

        internal void AddPostActions(ActionType type, ActionList actions)
        {
            _actions[type].AddPostActions(actions);
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

        internal void SetEntitySelector<TEntity>(ISelector selector)
            where TEntity : class
        {
            _entitySelectors[typeof(TEntity)] = selector;
        }

        internal void SetEntitySorter<TEntity>(ISorter sorter)
            where TEntity : class
        {
            _entitySorters[typeof(TEntity)] = sorter;
        }

        internal void SetEntityFilters<TEntity>(List<IFilter> filters)
        {
            _entityFilters[typeof(TEntity)] = filters;
        }

        internal void SetEntityCreator<TEntity>(
            Func<object, Task<TEntity>> creator)
            where TEntity : class
        {
            _entityCreators[typeof(TEntity)] = async request => await creator(request).Configure();
        }

        internal void SetEntitiesCreator<TEntity>(
            Func<object, Task<TEntity[]>> creator)
            where TEntity : class
        {
            _entitiesCreators[typeof(TEntity)] = async request => await creator(request).Configure();
        }

        internal void SetEntityUpdator<TEntity>(
            Func<object, TEntity, Task> updator)
            where TEntity : class
        {
            _entityUpdators[typeof(TEntity)] = (request, entity) => updator(request, (TEntity)entity);
        }

        internal void SetEntityDefault<TEntity>(
            TEntity defaultValue)
            where TEntity : class
        {
            _defaultValues[typeof(TEntity)] = defaultValue;
        }

        private void OverrideOptions(RequestOptions options, Type tEntity)
        {
            foreach (var type in tEntity.BuildTypeHierarchyDown())
            {
                if (_entityOptionOverrides.TryGetValue(type, out var entityOptions))
                    OverrideOptions(options, entityOptions);
            }
        }

        private void OverrideOptions(RequestOptions options, CrudOptionsConfig config)
        {
            if (config.SuppressCreateActionsInSave.HasValue)
                options.SuppressCreateActionsInSave = config.SuppressCreateActionsInSave.Value;

            if (config.SuppressUpdateActionsInSave.HasValue)
                options.SuppressUpdateActionsInSave = config.SuppressUpdateActionsInSave.Value;

            if (config.UseProjection.HasValue)
                options.UseProjection = config.UseProjection.Value;
        }
    }
}

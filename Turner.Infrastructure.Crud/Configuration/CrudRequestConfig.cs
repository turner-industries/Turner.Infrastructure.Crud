using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Exceptions;

namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudRequestConfig
    {
        ErrorConfig ErrorConfig { get; }

        RequestOptions GetOptionsFor<TEntity>()
            where TEntity : class;

        IKey GetRequestKey();

        IKey GetKeyFor<TEntity>()
            where TEntity : class;

        ISelector GetSelectorFor<TEntity>()
            where TEntity : class;

        ISorter GetSorterFor<TEntity>()
            where TEntity : class;

        IRequestData GetRequestDataFor<TEntity>()
            where TEntity : class;

        TEntity GetDefaultFor<TEntity>()
            where TEntity : class;

        IEnumerable<IFilter> GetFiltersFor<TEntity>();

        Task RunPreActionsFor<TEntity>(ActionType type, object item)
            where TEntity : class;

        Task RunPostActionsFor<TEntity>(ActionType type, object item, TEntity entity)
            where TEntity : class;

        Func<object, Task<TEntity>> GetCreatorFor<TEntity>()
            where TEntity : class;

        Func<object, TEntity, Task<TEntity>> GetUpdatorFor<TEntity>()
            where TEntity : class;

        IEnumerable<Tuple<object, TEntity>> Join<TEntity>(IEnumerable<object> items, IEnumerable<TEntity> entities)
            where TEntity : class;
    }

    public class CrudRequestConfig<TRequest>
        : ICrudRequestConfig
    {
        private IKey _requestKey;

        private readonly ActionConfig _actions = new ActionConfig();
        private readonly RequestOptions _options = new RequestOptions();

        private readonly Dictionary<Type, CrudOptionsConfig> _entityOptionOverrides
            = new Dictionary<Type, CrudOptionsConfig>();

        private readonly Dictionary<Type, RequestOptions> _optionsCache
            = new Dictionary<Type, RequestOptions>();

        private readonly Dictionary<Type, Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<Tuple<object, object>>>> _entityJoiners
            = new Dictionary<Type, Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<Tuple<object, object>>>>();

        private readonly Dictionary<Type, IRequestData> _entityRequestData
            = new Dictionary<Type, IRequestData>();

        private readonly Dictionary<Type, IKey> _entityKeys
            = new Dictionary<Type, IKey>();

        private readonly Dictionary<Type, ISorter> _entitySorters
            = new Dictionary<Type, ISorter>();

        private readonly Dictionary<Type, ISelector> _entitySelectors
            = new Dictionary<Type, ISelector>();

        private readonly Dictionary<Type, Func<object, Task<object>>> _entityCreators
            = new Dictionary<Type, Func<object, Task<object>>>();
        
        private readonly Dictionary<Type, Func<object, object, Task<object>>> _entityUpdators
            = new Dictionary<Type, Func<object, object, Task<object>>>();

        private readonly Dictionary<Type, Func<object, object[], Task<object[]>>> _entitiesUpdators
            = new Dictionary<Type, Func<object, object[], Task<object[]>>>();

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

        public IRequestData GetRequestDataFor<TEntity>()
            where TEntity : class
        {
            foreach (var type in typeof(TEntity).BuildTypeHierarchyUp())
            {
                if (_entityRequestData.TryGetValue(type, out var dataSource))
                    return dataSource;
            }

            throw new BadCrudConfigurationException(
                $"No data defined for entity '{typeof(TEntity)}' " +
                $"for request '{typeof(TRequest)}'.");
        }

        public IKey GetRequestKey() => _requestKey;

        public IKey GetKeyFor<TEntity>()
            where TEntity : class
        {
            foreach (var type in typeof(TEntity).BuildTypeHierarchyUp())
            {
                if (_entityKeys.TryGetValue(type, out var key))
                    return key;
            }

            return null;
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

        public Func<object, Task<TEntity>> GetCreatorFor<TEntity>()
            where TEntity : class
        {
            if (_entityCreators.TryGetValue(typeof(TEntity), out var creator))
                return async item => (TEntity) await creator(item).Configure();
            
            return item => Task.FromResult(Mapper.Map<TEntity>(item));
        }

        public Func<object, TEntity, Task<TEntity>> GetUpdatorFor<TEntity>()
            where TEntity : class
        {
            if (_entityUpdators.TryGetValue(typeof(TEntity), out var updator))
                return async (item, entity) => (TEntity) await updator(item, entity).Configure();

            return (item, entity) => Task.FromResult(Mapper.Map(item, entity));
        }
        
        public IEnumerable<Tuple<object, TEntity>> Join<TEntity>(
            IEnumerable<object> items, 
            IEnumerable<TEntity> entities)
            where TEntity : class
        {
            if (!_entityJoiners.TryGetValue(typeof(TEntity), out var joiner))
            {
                var message =
                    $"Unable to join entities of type '{entities.GetType()}' " +
                    $"with request items of type '{items.GetType()}'. ";

                throw new BadCrudConfigurationException(message);
            }

            return joiner(items, entities).Select(t => new Tuple<object, TEntity>(t.Item1, (TEntity)t.Item2));
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

        internal void SetEntityRequestData<TEntity>(IRequestData dataSource)
            where TEntity : class
        {
            _entityRequestData[typeof(TEntity)] = dataSource;
        }

        internal void SetRequestKey(IKey key)
        {
            _requestKey = key;
        }

        internal void SetEntityKey<TEntity>(IKey key)
            where TEntity : class
        {
            _entityKeys[typeof(TEntity)] = key;
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
            _entityCreators[typeof(TEntity)] = async item => await creator(item).Configure();
        }

        internal void SetEntityUpdator<TEntity>(
            Func<object, TEntity, Task<TEntity>> updator)
            where TEntity : class
        {
            _entityUpdators[typeof(TEntity)] = async (item, entity) => await updator(item, (TEntity) entity);
        }

        internal void SetEntitiesUpdator<TEntity>(
            Func<object, TEntity[], Task<TEntity[]>> updator)
            where TEntity : class
        {
            _entitiesUpdators[typeof(TEntity)] = async (request, entities) => await updator(request, (TEntity[]) entities).Configure();
        }

        internal void SetEntityJoiner<TEntity>(
            Func<IEnumerable<object>, IEnumerable<TEntity>, IEnumerable<Tuple<object, TEntity>>> joiner)
            where TEntity : class
        {
            _entityJoiners[typeof(TEntity)] = (x, y) => 
                joiner(x, y.Cast<TEntity>()).Select(t => new Tuple<object, object>(t.Item1, t.Item2));
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

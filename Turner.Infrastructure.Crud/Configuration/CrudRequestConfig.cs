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

        IRequestItemSource GetRequestItemSourceFor<TEntity>()
            where TEntity : class;

        TEntity GetDefaultFor<TEntity>()
            where TEntity : class;

        IEnumerable<IFilter> GetFiltersFor<TEntity>();

        List<IBoxedRequestHook> GetRequestHooks(object request);

        List<IBoxedEntityHook> GetEntityHooksFor<TEntity>(object request)
            where TEntity : class;

        List<IBoxedItemHook> GetItemHooksFor<TEntity>(object request)
            where TEntity : class;

        List<IBoxedResultHook> GetResultHooks(object request);

        Func<object, object, Task<TEntity>> GetCreatorFor<TEntity>()
            where TEntity : class;

        Func<object, object, TEntity, Task<TEntity>> GetUpdatorFor<TEntity>()
            where TEntity : class;

        Func<TEntity, Task<TResult>> GetResultCreatorFor<TEntity, TResult>()
            where TEntity : class;

        IEnumerable<Tuple<object, TEntity>> Join<TEntity>(IEnumerable<object> items, IEnumerable<TEntity> entities)
            where TEntity : class;
    }

    public class CrudRequestConfig<TRequest>
        : ICrudRequestConfig
    {
        private IKey _requestKey;

        private readonly RequestHookConfig<TRequest> _requestHooks = new RequestHookConfig<TRequest>();

        private readonly Dictionary<Type, EntityHookConfig<TRequest>> _entityHooks 
            = new Dictionary<Type, EntityHookConfig<TRequest>>();
        
        private readonly Dictionary<Type, ItemHookConfig<TRequest>> _itemHooks
            = new Dictionary<Type, ItemHookConfig<TRequest>>();

        private readonly ResultHookConfig<TRequest> _resultHooks = new ResultHookConfig<TRequest>();

        private readonly RequestOptions _options = new RequestOptions();

        private readonly Dictionary<Type, CrudRequestOptionsConfig> _entityOptionOverrides
            = new Dictionary<Type, CrudRequestOptionsConfig>();

        private readonly Dictionary<Type, RequestOptions> _optionsCache
            = new Dictionary<Type, RequestOptions>();

        private readonly Dictionary<Type, Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<Tuple<object, object>>>> _entityJoiners
            = new Dictionary<Type, Func<IEnumerable<object>, IEnumerable<object>, IEnumerable<Tuple<object, object>>>>();

        private readonly Dictionary<Type, IRequestItemSource> _entityRequestItemSources
            = new Dictionary<Type, IRequestItemSource>();

        private readonly Dictionary<Type, IKey> _entityKeys
            = new Dictionary<Type, IKey>();

        private readonly Dictionary<Type, ISorter> _entitySorters
            = new Dictionary<Type, ISorter>();

        private readonly Dictionary<Type, ISelector> _entitySelectors
            = new Dictionary<Type, ISelector>();

        private readonly Dictionary<Type, Func<object, object, Task<object>>> _entityCreators
            = new Dictionary<Type, Func<object, object, Task<object>>>();
        
        private readonly Dictionary<Type, Func<object, object, object, Task<object>>> _entityUpdators
            = new Dictionary<Type, Func<object, object, object, Task<object>>>();

        private readonly Dictionary<Type, Func<object, Task<object>>> _entityResultCreators
            = new Dictionary<Type, Func<object, Task<object>>>();

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

        public List<IBoxedRequestHook> GetRequestHooks(object request)
        {
            if (!(request is TRequest))
            {
                var message =
                    $"Unable to get request hooks for request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'.";

                throw new BadCrudConfigurationException(message);
            }

            return _requestHooks.GetHooks((TRequest)request);
        }

        public List<IBoxedEntityHook> GetEntityHooksFor<TEntity>(object request)
            where TEntity : class
        {
            if (!(request is TRequest))
            {
                var message =
                    $"Unable to get entity hooks for request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'.";

                throw new BadCrudConfigurationException(message);
            }

            var hooks = new List<IBoxedEntityHook>();

            foreach (var type in typeof(TEntity).BuildTypeHierarchyDown())
            {
                if (_entityHooks.TryGetValue(type, out var entityHooks))
                    hooks.AddRange(entityHooks.GetHooks((TRequest)request));
            }

            return hooks;
        }

        public List<IBoxedItemHook> GetItemHooksFor<TEntity>(object request)
            where TEntity : class
        {
            if (!(request is TRequest))
            {
                var message =
                    $"Unable to get item hooks for request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'.";

                throw new BadCrudConfigurationException(message);
            }

            var hooks = new List<IBoxedItemHook>();

            foreach (var type in typeof(TEntity).BuildTypeHierarchyDown())
            {
                if (_itemHooks.TryGetValue(type, out var itemHooks))
                    hooks.AddRange(itemHooks.GetHooks((TRequest)request));
            }

            return hooks;
        }

        public List<IBoxedResultHook> GetResultHooks(object request)
        {
            if (!(request is TRequest))
            {
                var message =
                    $"Unable to get result hooks for request of type '{request.GetType()}'. " +
                    $"Configuration expected a request of type '{typeof(TRequest)}'.";

                throw new BadCrudConfigurationException(message);
            }

            return _resultHooks.GetHooks((TRequest)request);
        }

        public IRequestItemSource GetRequestItemSourceFor<TEntity>()
            where TEntity : class
        {
            foreach (var type in typeof(TEntity).BuildTypeHierarchyUp())
            {
                if (_entityRequestItemSources.TryGetValue(type, out var itemSource))
                    return itemSource;
            }

            var source = RequestItemSource.From<TRequest, TRequest>(request => request);
            _entityRequestItemSources[typeof(TEntity)] = source;

            return source;
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

        public Func<object, object, Task<TEntity>> GetCreatorFor<TEntity>()
            where TEntity : class
        {
            if (_entityCreators.TryGetValue(typeof(TEntity), out var creator))
                return (request, item) => creator(request, item).ContinueWith(t => (TEntity) t.Result);
            
            return (request, item) => Task.FromResult(Mapper.Map<TEntity>(item));
        }

        public Func<object, object, TEntity, Task<TEntity>> GetUpdatorFor<TEntity>()
            where TEntity : class
        {
            if (_entityUpdators.TryGetValue(typeof(TEntity), out var updator))
                return (request, item, entity) => updator(request, item, entity).ContinueWith(t => (TEntity)t.Result);

            return (request, item, entity) => Task.FromResult(Mapper.Map(item, entity));
        }

        public Func<TEntity, Task<TResult>> GetResultCreatorFor<TEntity, TResult>()
            where TEntity : class
        {
            if (_entityResultCreators.TryGetValue(typeof(TEntity), out var creator))
                return entity => creator(entity).ContinueWith(t => (TResult)t.Result);

            return entity => Task.FromResult(Mapper.Map<TResult>(entity));
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

        internal void SetOptions(CrudRequestOptionsConfig options)
        {
            if (options != null)
                OverrideOptions(_options, options);
        }

        internal void SetOptionsFor<TEntity>(CrudRequestOptionsConfig options)
        {
            _entityOptionOverrides[typeof(TEntity)] = options;
        }

        internal void AddRequestHooks(List<IRequestHookFactory> hooks)
        {
            _requestHooks.AddHooks(hooks);
        }

        internal void SetEntityHooksFor<TEntity>(List<IEntityHookFactory> hooks)
            where TEntity : class
        {
            var config = new EntityHookConfig<TRequest>();
            config.SetHooks(hooks);

            _entityHooks[typeof(TEntity)] = config;
        }

        internal void SetItemHooksFor<TEntity>(List<IItemHookFactory> hooks)
        {
            var config = new ItemHookConfig<TRequest>();
            config.SetHooks(hooks);

            _itemHooks[typeof(TEntity)] = config;
        }

        internal void AddResultHooks(List<IResultHookFactory> hooks)
        {
            _resultHooks.AddHooks(hooks);
        }

        internal void SetEntityRequestItemSource<TEntity>(IRequestItemSource itemSource)
            where TEntity : class
        {
            _entityRequestItemSources[typeof(TEntity)] = itemSource;
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
            Func<object, object, Task<TEntity>> creator)
            where TEntity : class
        {
            _entityCreators[typeof(TEntity)] = (request, item) 
                => creator(request, item).ContinueWith(t => (object)t.Result);
        }

        internal void SetEntityUpdator<TEntity>(
            Func<object, object, TEntity, Task<TEntity>> updator)
            where TEntity : class
        {
            _entityUpdators[typeof(TEntity)] = (request, item, entity) 
                => updator(request, item, (TEntity)entity).ContinueWith(t => (object)t.Result);
        }

        internal void SetEntityResultCreator<TEntity>(
            Func<TEntity, Task<object>> creator)
            where TEntity : class
        {
            _entityResultCreators[typeof(TEntity)] = entity => creator((TEntity)entity);
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

        private void OverrideOptions(RequestOptions options, CrudRequestOptionsConfig config)
        {
            if (config.UseProjection.HasValue)
                options.UseProjection = config.UseProjection.Value;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration.Builders.Filter;
using Turner.Infrastructure.Crud.Configuration.Builders.Select;
using Turner.Infrastructure.Crud.Configuration.Builders.Sort;
using Turner.Infrastructure.Crud.Errors;

namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public interface ICrudRequestEntityConfigBuilder
    {
        void Build<TRequest>(CrudRequestConfig<TRequest> config);
    }

    public class CrudRequestEntityConfigBuilder<TRequest, TEntity>
        : ICrudRequestEntityConfigBuilder
        where TEntity : class
    {
        private readonly Dictionary<ActionType, List<Func<TRequest, Task>>> _preActions
            = new Dictionary<ActionType, List<Func<TRequest, Task>>>();

        private readonly Dictionary<ActionType, List<Func<TEntity, Task>>> _postActions
            = new Dictionary<ActionType, List<Func<TEntity, Task>>>();

        private readonly List<IFilter> _requestFilters = new List<IFilter>();

        private CrudOptionsConfig _optionsConfig;
        private TEntity _defaultValue;
        private ISorter _sortEntityFromRequest;
        private ISelector _selectEntityFromRequest;
        private Func<TRequest, Task<TEntity>> _createEntityFromRequest;
        private Func<TRequest, Task<TEntity[]>> _createEntitiesFromRequest;
        private Func<TRequest, TEntity, Task<TEntity>> _updateEntityFromRequest;
        private Func<TRequest, TEntity[], Task<TEntity[]>> _updateEntitiesFromRequest;
        private Func<ICrudErrorHandler> _errorHandlerFactory;

        public CrudRequestEntityConfigBuilder()
        {
            foreach (var type in (ActionType[]) Enum.GetValues(typeof(ActionType)))
            {
                _preActions[type] = new List<Func<TRequest, Task>>();
                _postActions[type] = new List<Func<TEntity, Task>>();
            }
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> ConfigureOptions(Action<CrudOptionsConfig> config)
        {
            if (config == null)
            {
                _optionsConfig = null;
            }
            else
            {
                _optionsConfig = new CrudOptionsConfig();
                config(_optionsConfig);
            }

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UseErrorHandlerFactory(Func<ICrudErrorHandler> handlerFactory)
        {
            _errorHandlerFactory = handlerFactory;

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeCreating(Func<TRequest, Task> action) 
            => AddPreAction(ActionType.Create, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeCreating(Action<TRequest> action)
            => AddPreAction(ActionType.Create, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterCreating(Func<TEntity, Task> action)
            => AddPostAction(ActionType.Create, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterCreating(Action<TEntity> action)
            => AddPostAction(ActionType.Create, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeUpdating(Func<TRequest, Task> action)
            => AddPreAction(ActionType.Update, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeUpdating(Action<TRequest> action)
            => AddPreAction(ActionType.Update, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterUpdating(Func<TEntity, Task> action)
            => AddPostAction(ActionType.Update, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterUpdating(Action<TEntity> action)
            => AddPostAction(ActionType.Update, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeDeleting(Func<TRequest, Task> action)
            => AddPreAction(ActionType.Delete, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeDeleting(Action<TRequest> action)
            => AddPreAction(ActionType.Delete, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterDeleting(Func<TEntity, Task> action) 
            => AddPostAction(ActionType.Delete, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterDeleting(Action<TEntity> action)
            => AddPostAction(ActionType.Delete, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeSaving(Func<TRequest, Task> action)
            => AddPreAction(ActionType.Save, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> BeforeSaving(Action<TRequest> action)
            => AddPreAction(ActionType.Save, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterSaving(Func<TEntity, Task> action)
            => AddPostAction(ActionType.Save, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> AfterSaving(Action<TEntity> action)
            => AddPostAction(ActionType.Save, action);

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UseDefault(TEntity defaultValue)
        {
            _defaultValue = defaultValue;

            return this;
        }
        
        public CrudRequestEntityConfigBuilder<TRequest, TEntity> FilterWith(
            Action<FilterBuilder<TRequest, TEntity>> build)
        {
            var builder = new FilterBuilder<TRequest, TEntity>();
            build(builder);

            return AddRequestFilter(builder.Build());
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SelectWith(
            Func<SelectorBuilder<TRequest, TEntity>, ISelector> build)
        {
            var builder = new SelectorBuilder<TRequest, TEntity>();
            _selectEntityFromRequest = build(builder);
            
            return this;
        }
        
        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SortWith(
            Action<SortBuilder<TRequest, TEntity>> build)
        {
            var builder = new SortBuilder<TRequest, TEntity>();
            build(builder);

            _sortEntityFromRequest = builder.Build();
            
            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> SortWith(
            Func<TRequest, IQueryable<TEntity>, IOrderedQueryable<TEntity>> sortFunc)
            => SortWith(builder => builder.Custom(sortFunc));

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> CreateWith(
            Func<TRequest, Task<TEntity>> creator)
        {
            _createEntityFromRequest = creator;

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> CreateWith(
            Func<TRequest, TEntity> creator)
        {
            _createEntityFromRequest = request => Task.FromResult(creator(request));

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> CreateAllWith(
            Func<TRequest, Task<TEntity[]>> creator)
        {
            _createEntitiesFromRequest = creator;

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> CreateAllWith(
            Func<TRequest, TEntity[]> creator)
        {
            _createEntitiesFromRequest = request => Task.FromResult(creator(request));

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateWith(
            Func<TRequest, TEntity, Task<TEntity>> updator)
        {
            _updateEntityFromRequest = updator;

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateWith(
            Action<TRequest, TEntity> updator)
        {
            _updateEntityFromRequest = (request, entity) =>
            {
                updator(request, entity);
                return Task.FromResult(entity);
            };

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateAllWith(
            Func<TRequest, TEntity[], Task<TEntity[]>> updator)
        {
            _updateEntitiesFromRequest = updator;

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateAllWith(
            Func<TRequest, TEntity[], TEntity[]> updator)
        {
            _updateEntitiesFromRequest = (request, entities) => Task.FromResult(updator(request, entities));

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateAllWith<TIn>(
            Expression<Func<TRequest, IEnumerable<TIn>>> requestEnumerableExpr,
            string requestItemKeyProperty,
            string entityKeyProperty,
            Func<TIn, TEntity, TEntity> updator)
        {
            FilterWith(builder =>
                builder.FilterOnCollection(requestEnumerableExpr, requestItemKeyProperty, entityKeyProperty));

            // TODO: Move this to a builder
            // TODO: Default to Mapper.Map for updator
            // TODO: Support Expression key properties
            // TODO: Support the key comparer version of Join?
            // TODO: Debugging features (active filters, etc)

            var rParamExpr = Expression.Parameter(typeof(TRequest));
            
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);

            var enumerableMethods = typeof(Enumerable).GetMethods();

            var tupleType = typeof(Tuple<,>).MakeGenericType(typeof(TIn), typeof(TEntity));
            var tupleCtor = tupleType.GetConstructor(new[] { typeof(TIn), typeof(TEntity) });
            
            var selectTupleParam = Expression.Parameter(tupleType);
            var item1Param = Expression.PropertyOrField(selectTupleParam, "Item1");
            var item2Param = Expression.PropertyOrField(selectTupleParam, "Item2");
            var updateExpr = Expression.Call(updator.Method, item1Param, item2Param);
            var selectLambdaExpr = Expression.Lambda(updateExpr, selectTupleParam);
            
            var esParamExpr = Expression.Parameter(typeof(TEntity[]));
            
            var joinInfo = enumerableMethods
                .Single(x => x.Name == "Join" && x.GetParameters().Length == 5)
                .MakeGenericMethod(typeof(TEntity), typeof(TIn), eKeyExpr.Type, tupleType);
            var joinEntityParam = Expression.Parameter(typeof(TEntity));
            var joinEntityKeyExpr = Expression.Lambda(
                Expression.PropertyOrField(joinEntityParam, entityKeyProperty),
                joinEntityParam);
            var joinInParam = Expression.Parameter(typeof(TIn));
            var joinInKeyExpr = Expression.Lambda(
                Expression.PropertyOrField(joinInParam, requestItemKeyProperty),
                joinInParam);
            var joinOutEntityParam = Expression.Parameter(typeof(TEntity));
            var joinOutInParam = Expression.Parameter(typeof(TIn));
            var joinOutExpr = Expression.Lambda(
                Expression.New(tupleCtor, joinOutInParam, joinOutEntityParam), 
                joinOutEntityParam, 
                joinOutInParam);
            var joinInEnumParam = Expression.Invoke(requestEnumerableExpr, rParamExpr);
            var joinExpr = Expression.Call(joinInfo, esParamExpr, joinInEnumParam, joinEntityKeyExpr, joinInKeyExpr, joinOutExpr);

            var selectInfo = enumerableMethods
                .Single(x => x.Name == "Select" &&
                                x.GetParameters().Length == 2 &&
                                x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                .MakeGenericMethod(tupleType, typeof(TEntity));
            var selectExpr = Expression.Call(selectInfo,  joinExpr, selectLambdaExpr);

            var toArrayInfo = enumerableMethods
                .Single(x => x.Name == "ToArray" && x.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(TEntity));
            var toArrayExpr = Expression.Call(toArrayInfo, selectExpr);

            var lambdaExpr = Expression.Lambda(toArrayExpr, rParamExpr, esParamExpr);
            var lambda = (Func<TRequest, TEntity[], TEntity[]>) lambdaExpr.Compile();

            _updateEntitiesFromRequest = (request, entities) => Task.FromResult(lambda(request, entities));

            return this;
        }

        public void Build<TCompatibleRequest>(CrudRequestConfig<TCompatibleRequest> config)
        {
            if (_optionsConfig != null)
                config.SetOptionsFor<TEntity>(_optionsConfig);

            if (_errorHandlerFactory != null)
                config.ErrorConfig.SetErrorHandlerFor(typeof(TEntity), _errorHandlerFactory);

            config.SetEntityDefault(_defaultValue);

            if (_selectEntityFromRequest != null)
                config.SetEntitySelector<TEntity>(_selectEntityFromRequest);

            if (_createEntityFromRequest != null)
                config.SetEntityCreator(request => _createEntityFromRequest((TRequest)request));

            if (_createEntitiesFromRequest != null)
                config.SetEntitiesCreator(request => _createEntitiesFromRequest((TRequest)request));

            if (_updateEntityFromRequest != null)
                config.SetEntityUpdator<TEntity>((request, entity) => _updateEntityFromRequest((TRequest)request, entity));

            if (_updateEntitiesFromRequest != null)
                config.SetEntitiesUpdator<TEntity>((request, entities) => _updateEntitiesFromRequest((TRequest)request, entities));

            if (_sortEntityFromRequest != null)
                config.SetEntitySorter<TEntity>(_sortEntityFromRequest);

            if (_requestFilters.Count > 0)
                config.SetEntityFilters<TEntity>(_requestFilters);

            foreach (var type in (ActionType[]) Enum.GetValues(typeof(ActionType)))
            {
                Func<object, Task> ConvertAction<TArg>(Func<TArg, Task> action)
                    => x => action((TArg) x);

                var preActions = _preActions[type];
                if (preActions.Count > 0)
                {
                    config.SetPreActionsFor<TEntity>(type, 
                        new ActionList(preActions.Select(ConvertAction<TRequest>)));
                }

                var postActions = _postActions[type];
                if (postActions.Count > 0)
                {
                    config.SetPostActionsFor<TEntity>(type,
                        new ActionList(postActions.Select(ConvertAction<TEntity>)));
                }
            }
        }

        private CrudRequestEntityConfigBuilder<TRequest, TEntity> AddRequestFilter(IFilter filter)
        {
            if (filter != null)
                _requestFilters.Add(filter);

            return this;
        }

        private CrudRequestEntityConfigBuilder<TRequest, TEntity> AddPreAction(ActionType type, Func<TRequest, Task> action)
        {
            if (action != null)
                _preActions[type].Add(action);

            return this;
        }

        private CrudRequestEntityConfigBuilder<TRequest, TEntity> AddPreAction(ActionType type, Action<TRequest> action)
        {
            if (action != null)
                _preActions[type].Add(request =>
                {
                    action(request);
                    return Task.CompletedTask;
                });

            return this;
        }

        private CrudRequestEntityConfigBuilder<TRequest, TEntity> AddPostAction(ActionType type, Func<TEntity, Task> action)
        {
            if (action != null)
                _postActions[type].Add(action);

            return this;
        }

        private CrudRequestEntityConfigBuilder<TRequest, TEntity> AddPostAction(ActionType type, Action<TEntity> action)
        {
            if (action != null)
                _postActions[type].Add(request =>
                {
                    action(request);
                    return Task.CompletedTask;
                });

            return this;
        }
    }
}

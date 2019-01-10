using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration.Builders.Filter;
using Turner.Infrastructure.Crud.Configuration.Builders.Select;
using Turner.Infrastructure.Crud.Configuration.Builders.Sort;
using Turner.Infrastructure.Crud.Errors;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public abstract class CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        : ICrudRequestEntityConfigBuilder
        where TEntity : class
        where TBuilder : CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
    {
        private readonly Dictionary<ActionType, List<Func<TRequest, Task>>> _preActions
            = new Dictionary<ActionType, List<Func<TRequest, Task>>>();

        private readonly Dictionary<ActionType, List<Func<TEntity, Task>>> _postActions
            = new Dictionary<ActionType, List<Func<TEntity, Task>>>();

        private readonly List<IFilter> _filters = new List<IFilter>();

        protected CrudOptionsConfig OptionsConfig;
        protected TEntity DefaultValue;
        protected ISorter Sorter;
        protected ISelector Selector;
        protected IRequestData RequestDataSource;
        protected Key EntityKey;
        protected Key RequestItemKey;
        protected Func<object, Task<TEntity>> CreateEntity;
        protected Func<object, TEntity, Task<TEntity>> UpdateEntity;
        protected Func<ICrudErrorHandler> ErrorHandlerFactory;

        public CrudRequestEntityConfigBuilderCommon()
        {
            foreach (var type in (ActionType[]) Enum.GetValues(typeof(ActionType)))
            {
                _preActions[type] = new List<Func<TRequest, Task>>();
                _postActions[type] = new List<Func<TEntity, Task>>();
            }
        }

        public TBuilder ConfigureOptions(Action<CrudOptionsConfig> config)
        {
            if (config == null)
            {
                OptionsConfig = null;
            }
            else
            {
                OptionsConfig = new CrudOptionsConfig();
                config(OptionsConfig);
            }

            return (TBuilder)this;
        }

        public TBuilder UseErrorHandlerFactory(Func<ICrudErrorHandler> handlerFactory)
        {
            ErrorHandlerFactory = handlerFactory;

            return (TBuilder)this;
        }

        public TBuilder BeforeCreating(Func<TRequest, Task> action) 
            => AddPreAction(ActionType.Create, action);

        public TBuilder BeforeCreating(Action<TRequest> action)
            => AddPreAction(ActionType.Create, action);

        public TBuilder AfterCreating(Func<TEntity, Task> action)
            => AddPostAction(ActionType.Create, action);

        public TBuilder AfterCreating(Action<TEntity> action)
            => AddPostAction(ActionType.Create, action);

        public TBuilder BeforeUpdating(Func<TRequest, Task> action)
            => AddPreAction(ActionType.Update, action);

        public TBuilder BeforeUpdating(Action<TRequest> action)
            => AddPreAction(ActionType.Update, action);

        public TBuilder AfterUpdating(Func<TEntity, Task> action)
            => AddPostAction(ActionType.Update, action);

        public TBuilder AfterUpdating(Action<TEntity> action)
            => AddPostAction(ActionType.Update, action);

        public TBuilder BeforeDeleting(Func<TRequest, Task> action)
            => AddPreAction(ActionType.Delete, action);

        public TBuilder BeforeDeleting(Action<TRequest> action)
            => AddPreAction(ActionType.Delete, action);

        public TBuilder AfterDeleting(Func<TEntity, Task> action) 
            => AddPostAction(ActionType.Delete, action);

        public TBuilder AfterDeleting(Action<TEntity> action)
            => AddPostAction(ActionType.Delete, action);

        public TBuilder BeforeSaving(Func<TRequest, Task> action)
            => AddPreAction(ActionType.Save, action);

        public TBuilder BeforeSaving(Action<TRequest> action)
            => AddPreAction(ActionType.Save, action);

        public TBuilder AfterSaving(Func<TEntity, Task> action)
            => AddPostAction(ActionType.Save, action);

        public TBuilder AfterSaving(Action<TEntity> action)
            => AddPostAction(ActionType.Save, action);
        
        public TBuilder WithEntityKey<TKey>(Expression<Func<TEntity, TKey>> entityKeyExpr)
        {
            EntityKey = new Key(typeof(TKey), entityKeyExpr);

            return (TBuilder)this;
        }

        public TBuilder WithEntityKey(string entityKeyProperty)
        {
            var eParamExpr = Expression.Parameter(typeof(TEntity));
            var eKeyExpr = Expression.PropertyOrField(eParamExpr, entityKeyProperty);

            EntityKey = new Key(
                ((PropertyInfo)eKeyExpr.Member).PropertyType,
                Expression.Lambda(eKeyExpr, eParamExpr));

            return (TBuilder)this;
        }
        
        public TBuilder UseDefault(TEntity defaultValue)
        {
            DefaultValue = defaultValue;

            return (TBuilder)this;
        }
        
        public TBuilder FilterWith(
            Action<FilterBuilder<TRequest, TEntity>> build)
        {
            var builder = new FilterBuilder<TRequest, TEntity>();
            build(builder);

            return AddRequestFilter(builder.Build());
        }

        public TBuilder SelectWith(
            Func<SelectorBuilder<TRequest, TEntity>, ISelector> build)
        {
            Selector = build(new SelectorBuilder<TRequest, TEntity>());
            
            return (TBuilder)this;
        }
        
        public TBuilder SortWith(
            Action<SortBuilder<TRequest, TEntity>> build)
        {
            var builder = new SortBuilder<TRequest, TEntity>();
            build(builder);

            Sorter = builder.Build();
            
            return (TBuilder)this;
        }

        public TBuilder SortWith(
            Func<TRequest, IQueryable<TEntity>, IOrderedQueryable<TEntity>> sortFunc)
            => SortWith(builder => builder.Custom(sortFunc));
        
        public virtual void Build<TCompatibleRequest>(CrudRequestConfig<TCompatibleRequest> config)
        {
            if (OptionsConfig != null)
                config.SetOptionsFor<TEntity>(OptionsConfig);

            if (ErrorHandlerFactory != null)
                config.ErrorConfig.SetErrorHandlerFor(typeof(TEntity), ErrorHandlerFactory);

            config.SetEntityDefault(DefaultValue);

            if (RequestItemKey != null)
                config.SetRequestKey(RequestItemKey);

            if (EntityKey != null)
                config.SetEntityKey<TEntity>(EntityKey);

            if (RequestDataSource != null)
                config.SetEntityRequestData<TEntity>(RequestDataSource);
                
            if (Selector != null)
                config.SetEntitySelector<TEntity>(Selector);
            
            if (CreateEntity != null)
                config.SetEntityCreator(CreateEntity);
            
            if (UpdateEntity != null)
                config.SetEntityUpdator(UpdateEntity);
            
            if (Sorter != null)
                config.SetEntitySorter<TEntity>(Sorter);

            if (_filters.Count > 0)
                config.SetEntityFilters<TEntity>(_filters);

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

        private TBuilder AddRequestFilter(IFilter filter)
        {
            if (filter != null)
                _filters.Add(filter);

            return (TBuilder)this;
        }

        private TBuilder AddPreAction(ActionType type, Func<TRequest, Task> action)
        {
            if (action != null)
                _preActions[type].Add(action);

            return (TBuilder)this;
        }

        private TBuilder AddPreAction(ActionType type, Action<TRequest> action)
        {
            if (action != null)
                _preActions[type].Add(request =>
                {
                    action(request);
                    return Task.CompletedTask;
                });

            return (TBuilder)this;
        }

        private TBuilder AddPostAction(ActionType type, Func<TEntity, Task> action)
        {
            if (action != null)
                _postActions[type].Add(action);

            return (TBuilder)this;
        }

        private TBuilder AddPostAction(ActionType type, Action<TEntity> action)
        {
            if (action != null)
                _postActions[type].Add(request =>
                {
                    action(request);
                    return Task.CompletedTask;
                });

            return (TBuilder)this;
        }
    }
}

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
        private readonly List<IFilter> _filters = new List<IFilter>();

        protected readonly List<IEntityHookFactory> EntityHooks
            = new List<IEntityHookFactory>();

        protected CrudRequestOptionsConfig OptionsConfig;
        protected TEntity DefaultValue;
        protected ISorter Sorter;
        protected ISelector Selector;
        protected IRequestItemSource RequestItemSource;
        protected Key EntityKey;
        protected Key RequestItemKey;
        protected Func<object, object, Task<TEntity>> CreateEntity;
        protected Func<object, object, TEntity, Task<TEntity>> UpdateEntity;
        protected Func<TEntity, Task<object>> CreateResult;
        protected Func<ICrudErrorHandler> ErrorHandlerFactory;
        
        public TBuilder ConfigureOptions(Action<CrudRequestOptionsConfig> config)
        {
            if (config == null)
            {
                OptionsConfig = null;
            }
            else
            {
                OptionsConfig = new CrudRequestOptionsConfig();
                config(OptionsConfig);
            }

            return (TBuilder)this;
        }

        public TBuilder UseErrorHandlerFactory(Func<ICrudErrorHandler> handlerFactory)
        {
            ErrorHandlerFactory = handlerFactory;

            return (TBuilder)this;
        }

        public TBuilder WithEntityHook<THook>()
            where THook : IEntityHook<TRequest, TEntity>
        {
            EntityHooks.Add(TypeEntityHookFactory.From<THook, TRequest, TEntity>());

            return (TBuilder)this;
        }
        
        public TBuilder WithEntityHook(IEntityHook<TRequest, TEntity> hook)
        {
            EntityHooks.Add(InstanceEntityHookFactory.From(hook));

            return (TBuilder)this;
        }

        public TBuilder WithEntityHook(Func<TRequest, TEntity, Task> hook)
        {
            EntityHooks.Add(FunctionEntityHookFactory.From(hook));

            return (TBuilder)this;
        }

        public TBuilder WithEntityHook(Action<TRequest, TEntity> hook)
        {
            EntityHooks.Add(FunctionEntityHookFactory.From(hook));

            return (TBuilder)this;
        }

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
        
        public TBuilder WithDefault(TEntity defaultValue)
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

        public TBuilder CreateResultWith<TResult>(
            Func<TEntity, Task<TResult>> creator)
        {
            CreateResult = entity => creator(entity).ContinueWith(t => (object)t.Result);

            return (TBuilder)this;
        }

        public TBuilder CreateResultWith<TResult>(
            Func<TEntity, TResult> creator)
        {
            CreateResult = entity => Task.FromResult((object)creator(entity));

            return (TBuilder)this;
        }

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

            if (RequestItemSource != null)
                config.SetEntityRequestItemSource<TEntity>(RequestItemSource);
                
            if (Selector != null)
                config.SetEntitySelector<TEntity>(Selector);
            
            if (CreateEntity != null)
                config.SetEntityCreator(CreateEntity);
            
            if (UpdateEntity != null)
                config.SetEntityUpdator(UpdateEntity);

            if (CreateResult != null)
                config.SetEntityResultCreator(CreateResult);
            
            if (Sorter != null)
                config.SetEntitySorter<TEntity>(Sorter);

            if (_filters.Count > 0)
                config.SetEntityFilters<TEntity>(_filters);

            config.SetEntityHooksFor<TEntity>(EntityHooks);
        }

        private TBuilder AddRequestFilter(IFilter filter)
        {
            if (filter != null)
                _filters.Add(filter);

            return (TBuilder)this;
        }
    }
}

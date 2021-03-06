﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration.Builders.Select;
using Turner.Infrastructure.Crud.Configuration.Builders.Sort;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public abstract class CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        : ICrudRequestEntityConfigBuilder
        where TEntity : class
        where TBuilder : CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
    {
        private readonly List<IFilterFactory> _filters = new List<IFilterFactory>();

        protected readonly List<IEntityHookFactory> EntityHooks
            = new List<IEntityHookFactory>();

        protected CrudRequestOptionsConfig OptionsConfig;
        protected TEntity DefaultValue;
        protected ISorterFactory Sorter;
        protected ISelector Selector;
        protected IRequestItemSource RequestItemSource;
        protected Key EntityKey;
        protected Key RequestItemKey;
        protected Func<object, object, CancellationToken, Task<TEntity>> CreateEntity;
        protected Func<object, object, TEntity, CancellationToken, Task<TEntity>> UpdateEntity;
        protected Func<TEntity, CancellationToken, Task<object>> CreateResult;
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

        public TBuilder AddEntityHook<THook, TBaseRequest, TBaseEntity>()
            where TBaseEntity : class
            where THook : IEntityHook<TBaseRequest, TBaseEntity>
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(AddEntityHook), typeof(TBaseRequest), typeof(TRequest));

            if (!typeof(TBaseEntity).IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(AddEntityHook), typeof(TBaseEntity), typeof(TEntity));

            EntityHooks.Add(TypeEntityHookFactory.From<THook, TBaseRequest, TBaseEntity>());

            return (TBuilder)this;
        }

        public TBuilder AddEntityHook<THook, TBaseRequest>()
            where THook : IEntityHook<TBaseRequest, TEntity>
            => AddEntityHook<THook, TBaseRequest, TEntity>();

        public TBuilder AddEntityHook<THook>()
            where THook : IEntityHook<TRequest, TEntity>
            => AddEntityHook<THook, TRequest, TEntity>();

        public TBuilder AddEntityHook<TBaseRequest, TBaseEntity>(IEntityHook<TBaseRequest, TBaseEntity> hook)
            where TBaseEntity : class
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(FilterWith), typeof(TBaseRequest), typeof(TRequest));

            if (!typeof(TBaseEntity).IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(FilterWith), typeof(TBaseEntity), typeof(TEntity));

            EntityHooks.Add(InstanceEntityHookFactory.From(hook));

            return (TBuilder)this;
        }

        public TBuilder AddEntityHook(Func<TRequest, TEntity, CancellationToken, Task> hook)
        {
            EntityHooks.Add(FunctionEntityHookFactory.From(hook));

            return (TBuilder)this;
        }

        public TBuilder AddEntityHook(Func<TRequest, TEntity, Task> hook)
            => AddEntityHook((request, entity, ct) => hook(request, entity));

        public TBuilder AddEntityHook(Action<TRequest, TEntity> hook)
        {
            EntityHooks.Add(FunctionEntityHookFactory.From(hook));

            return (TBuilder)this;
        }

        public TBuilder UseEntityKey<TKey>(Expression<Func<TEntity, TKey>> entityKeyExpr)
        {
            EntityKey = new Key(typeof(TKey), entityKeyExpr);

            return (TBuilder)this;
        }

        public TBuilder UseEntityKey(string entityKeyProperty)
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

        public TBuilder SelectWith(
            Func<SelectorBuilder<TRequest, TEntity>, ISelector> build)
        {
            Selector = build(new SelectorBuilder<TRequest, TEntity>());

            return (TBuilder)this;
        }
        
        public TBuilder FilterWith<TFilter, TBaseRequest, TBaseEntity>()
            where TBaseEntity : class
            where TFilter : IFilter<TBaseRequest, TBaseEntity>
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(FilterWith), typeof(TBaseRequest), typeof(TRequest));

            if (!typeof(TBaseEntity).IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(FilterWith), typeof(TBaseEntity), typeof(TEntity));

            return AddRequestFilter(TypeFilterFactory.From<TFilter, TBaseRequest, TBaseEntity>());
        }

        public TBuilder FilterWith<TFilter, TBaseRequest>()
            where TFilter : IFilter<TBaseRequest, TEntity>
            => FilterWith<TFilter, TBaseRequest, TEntity>();

        public TBuilder FilterWith<TFilter>()
            where TFilter : IFilter<TRequest, TEntity>
            => FilterWith<TFilter, TRequest, TEntity>();

        public TBuilder FilterWith<TBaseRequest, TBaseEntity>(IFilter<TBaseRequest, TBaseEntity> filter)
            where TBaseEntity : class
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(FilterWith), typeof(TBaseRequest), typeof(TRequest));

            if (!typeof(TBaseEntity).IsAssignableFrom(typeof(TEntity)))
                throw new ContravarianceException(nameof(FilterWith), typeof(TBaseEntity), typeof(TEntity));

            return AddRequestFilter(InstanceFilterFactory.From(filter));
        }

        public TBuilder FilterWith<TBaseRequest>(
            Func<TBaseRequest, IQueryable<TEntity>, IQueryable<TEntity>> filterFunc)
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(FilterWith), typeof(TBaseRequest), typeof(TRequest));

            return AddRequestFilter(FunctionFilterFactory.From(filterFunc));
        }

        public TBuilder FilterWith(
            Func<TRequest, IQueryable<TEntity>, IQueryable<TEntity>> filterFunc)
            => FilterWith<TRequest>(filterFunc);

        public TBuilder SortWith(
            Action<SortBuilder<TRequest, TEntity>> build)
        {
            var builder = new SortBuilder<TRequest, TEntity>();
            build(builder);

            Sorter = builder.Build();
            
            return (TBuilder)this;
        }
        
        public TBuilder SortWith<TSorter, TBaseRequest>()
            where TSorter : ISorter<TBaseRequest, TEntity>
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(SortWith), typeof(TBaseRequest), typeof(TRequest));
            
            Sorter = TypeSorterFactory.From<TSorter, TBaseRequest, TEntity>();

            return (TBuilder)this;
        }
        
        public TBuilder SortWith<TSorter>()
            where TSorter : ISorter<TRequest, TEntity>
            => SortWith<TSorter, TRequest>();

        public TBuilder SortWith<TBaseRequest>(ISorter<TBaseRequest, TEntity> sorter)
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(SortWith), typeof(TBaseRequest), typeof(TRequest));

            Sorter = InstanceSorterFactory.From(sorter);

            return (TBuilder)this;
        }

        public TBuilder SortUsing<TBaseRequest>(
            Func<TBaseRequest, IQueryable<TEntity>, IOrderedQueryable<TEntity>> sortFunc)
        {
            if (!typeof(TBaseRequest).IsAssignableFrom(typeof(TRequest)))
                throw new ContravarianceException(nameof(SortUsing), typeof(TBaseRequest), typeof(TRequest));

            Sorter = FunctionSorterFactory.From(sortFunc);

            return (TBuilder)this;
        }

        public TBuilder SortUsing(Func<TRequest, IQueryable<TEntity>, IOrderedQueryable<TEntity>> sortFunc)
            => SortUsing<TRequest>(sortFunc);

        public TBuilder CreateResultWith<TResult>(
            Func<TEntity, CancellationToken, Task<TResult>> creator)
        {
            CreateResult = (entity, ct) => creator(entity, ct).ContinueWith(t => (object)t.Result);

            return (TBuilder)this;
        }

        public TBuilder CreateResultWith<TResult>(
            Func<TEntity, Task<TResult>> creator)
            => CreateResultWith((entity, ct) => creator(entity));

        public TBuilder CreateResultWith<TResult>(
            Func<TEntity, TResult> creator)
        {
            CreateResult = (entity, ct) =>
            {
                if (ct.IsCancellationRequested)
                    return Task.FromCanceled<object>(ct);

                return Task.FromResult((object)creator(entity));
            };

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

        private TBuilder AddRequestFilter(IFilterFactory filter)
        {
            if (filter != null)
                _filters.Add(filter);

            return (TBuilder)this;
        }
    }
}

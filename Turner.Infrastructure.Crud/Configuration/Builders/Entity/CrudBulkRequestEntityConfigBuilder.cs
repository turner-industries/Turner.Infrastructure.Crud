using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration.Builders.Select;
using Turner.Infrastructure.Crud.Exceptions;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public class CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>
        : CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>>
        where TEntity : class
    {
        private Expression<Func<TRequest, IEnumerable<TItem>>> _getRequestItems;

        private readonly List<IItemHookFactory> _itemHooks
            = new List<IItemHookFactory>();
        
        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> WithItems(
            Expression<Func<TRequest, IEnumerable<TItem>>> requestItemsExpr)
        {
            _getRequestItems = requestItemsExpr;
            RequestItemSource = Infrastructure.Crud.RequestItemSource.From(BuildItemSource(requestItemsExpr));

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> WithItemHook<THook>()
            where THook : IItemHook<TRequest, TItem>
        {
            _itemHooks.Add(TypeItemHookFactory.From<THook, TRequest, TItem>());

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> WithItemHook(
            IItemHook<TRequest, TItem> hook)
        {
            _itemHooks.Add(InstanceItemHookFactory.From(hook));

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> WithItemHook(
            Func<TRequest, TItem, Task<TItem>> hook)
        {
            _itemHooks.Add(FunctionItemHookFactory.From(hook));

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> WithItemHook(
            Func<TRequest, TItem, TItem> hook)
        {
            _itemHooks.Add(FunctionItemHookFactory.From(hook));

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> WithRequestKey<TKey>(
            Expression<Func<TItem, TKey>> itemKeyExpr)
        {
            RequestItemKey = new Key(typeof(TKey), itemKeyExpr);

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> WithRequestKey(
            string itemKeyProperty)
        {
            var iParamExpr = Expression.Parameter(typeof(TItem));
            var iKeyExpr = Expression.PropertyOrField(iParamExpr, itemKeyProperty);

            RequestItemKey = new Key(
                ((PropertyInfo)iKeyExpr.Member).PropertyType,
                Expression.Lambda(iKeyExpr, iParamExpr));

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<TRequest, TItem, Task<TEntity>> creator)
        {
            CreateEntity = (request, item) => creator((TRequest)request, (TItem)item);

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<TRequest, TItem, TEntity> creator)
        {
            CreateEntity = (request, item) => Task.FromResult(creator((TRequest)request, (TItem)item));

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<TItem, Task<TEntity>> creator)
        {
            CreateEntity = (request, item) => creator((TItem)item);

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> CreateEntityWith(
            Func<TItem, TEntity> creator)
        {
            CreateEntity = (request, item) => Task.FromResult(creator((TItem)item));

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<TRequest, TItem, TEntity, Task<TEntity>> updator)
        {
            UpdateEntity = (request, item, entity) => updator((TRequest)request, (TItem)item, entity);

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<TRequest, TItem, TEntity, TEntity> updator)
        {
            UpdateEntity = (request, item, entity) =>
            {
                updator((TRequest)request, (TItem)item, entity);
                return Task.FromResult(entity);
            };

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<TItem, TEntity, Task<TEntity>> updator)
        {
            UpdateEntity = (request, item, entity) => updator((TItem)item, entity);

            return this;
        }

        public CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> UpdateEntityWith(
            Func<TItem, TEntity, TEntity> updator)
        {
            UpdateEntity = (request, item, entity) =>
            {
                updator((TItem)item, entity);
                return Task.FromResult(entity);
            };

            return this;
        }

        public override void Build<TCompatibleRequest>(CrudRequestConfig<TCompatibleRequest> config)
        {
            if (_getRequestItems == null)
            {
                var message =
                    $"No request item source has been defined for '{typeof(TRequest)}'." +
                    $"Define item source by calling `{nameof(WithItems)}` in the request's profile.";

                throw new BadCrudConfigurationException(message);
            }

            base.Build(config);

            if (Selector == null)
                DefaultSelector(config);

            BuildJoiner(config);

            config.SetItemHooksFor<TEntity>(_itemHooks);
        }

        private Func<TRequest, object> BuildItemSource(Expression<Func<TRequest, IEnumerable<TItem>>> itemsExpr)
        {
            var enumerableMethods = typeof(Enumerable).GetMethods();
            var rParamExpr = Expression.Parameter(typeof(TRequest));

            var castInfo = enumerableMethods
                .Single(x => x.Name == "Cast" && x.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(object));
            var castExpr = Expression.Call(castInfo,
                Expression.Invoke(itemsExpr, rParamExpr));

            var toArrayInfo = enumerableMethods
                .Single(x => x.Name == "ToArray" && x.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(object));
            var toArrayExpr = Expression.Call(toArrayInfo, castExpr);

            var lambdaExpr = Expression.Lambda<Func<TRequest, object>>(
                Expression.Convert(toArrayExpr, typeof(object)), rParamExpr);

            return lambdaExpr.Compile();
        }

        private void DefaultSelector<TCompatibleRequest>(
            CrudRequestConfig<TCompatibleRequest> config)
        {
            var itemKey = config.GetRequestKey();
            var entityKey = config.GetKeyFor<TEntity>();

            if (itemKey != null && entityKey != null)
            {
                var builder = new SelectorBuilder<TRequest, TEntity>();
                config.SetEntitySelector<TEntity>(builder.Collection(_getRequestItems, entityKey, itemKey));
            }
        }

        private void BuildJoiner<TCompatibleRequest>(CrudRequestConfig<TCompatibleRequest> config)
        {
            if (EntityKey == null || RequestItemKey == null)
                return;
            
            var joinInfo = typeof(EnumerableExtensions)
                .GetMethod("FullOuterJoin", BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(typeof(object), typeof(TEntity), RequestItemKey.KeyType);

            var makeKeySelectorInfo = typeof(CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>)
                .GetMethod("MakeKeySelector", BindingFlags.Static | BindingFlags.NonPublic);

            var itemsParam = Expression.Parameter(typeof(IEnumerable<object>));
            var entitiesParam = Expression.Parameter(typeof(IEnumerable<TEntity>));

            var makeLeftKeySelector = makeKeySelectorInfo.MakeGenericMethod(typeof(object), RequestItemKey.KeyType);
            var convLeftKeyParam = Expression.Parameter(typeof(object));
            var convLeftKeyCall = Expression.Invoke(
                RequestItemKey.KeyExpression, 
                Expression.Convert(convLeftKeyParam, typeof(TItem)));
            var leftKeyExpr = Expression.Call(makeLeftKeySelector, Expression.Lambda(convLeftKeyCall, convLeftKeyParam));

            var makeRightKeySelector = makeKeySelectorInfo.MakeGenericMethod(typeof(TEntity), EntityKey.KeyType);
            var rightKeyExpr = Expression.Call(makeRightKeySelector, Expression.Constant(EntityKey.KeyExpression));
            
            var joinExpr = Expression.Call(joinInfo, itemsParam, entitiesParam, leftKeyExpr, rightKeyExpr);
            var lambdaExpr = Expression.Lambda<Func<IEnumerable<object>, IEnumerable<TEntity>, IEnumerable<Tuple<object, TEntity>>>>(
                joinExpr, itemsParam, entitiesParam);

            config.SetEntityJoiner(lambdaExpr.Compile());
        }

        private static Func<T, TKey> MakeKeySelector<T, TKey>(LambdaExpression selector)
        {
            var tParam = Expression.Parameter(typeof(T));
            var invokeExpr = Expression.Invoke(selector, tParam);
            return Expression.Lambda<Func<T, TKey>>(invokeExpr, tParam).Compile();
        }
    }
}

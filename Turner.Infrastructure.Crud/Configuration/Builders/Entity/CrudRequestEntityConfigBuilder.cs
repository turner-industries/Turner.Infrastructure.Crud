using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration.Builders.Select;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public class CrudRequestEntityConfigBuilder<TRequest, TEntity>
        : CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, CrudRequestEntityConfigBuilder<TRequest, TEntity>>
        where TEntity : class
    {
        public CrudRequestEntityConfigBuilder<TRequest, TEntity> WithRequestKey<TKey>(
            Expression<Func<TRequest, TKey>> requestItemKeyExpr)
        {
            RequestItemKey = new Key(typeof(TKey), requestItemKeyExpr);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> WithRequestKey(string requestKeyProperty)
        {
            var rParamExpr = Expression.Parameter(typeof(TRequest));
            var rKeyExpr = Expression.PropertyOrField(rParamExpr, requestKeyProperty);

            RequestItemKey = new Key(
                ((PropertyInfo)rKeyExpr.Member).PropertyType,
                Expression.Lambda(rKeyExpr, rParamExpr));

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> CreateEntityWith(
            Func<TRequest, CancellationToken, Task<TEntity>> creator)
        {
            CreateEntity = (request, item, ct) => creator((TRequest)item, ct);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> CreateEntityWith(
            Func<TRequest, Task<TEntity>> creator)
            => CreateEntityWith((request, ct) => creator(request));

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> CreateEntityWith(
            Func<TRequest, TEntity> creator)
        {
            CreateEntity = (request, item, ct) =>
            {
                if (ct.IsCancellationRequested)
                    return Task.FromCanceled<TEntity>(ct);

                return Task.FromResult(creator((TRequest)item));
            };

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateEntityWith(
            Func<TRequest, TEntity, CancellationToken, Task<TEntity>> updator)
        {
            UpdateEntity = (request, item, entity, ct) => updator((TRequest)item, entity, ct);

            return this;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateEntityWith(
            Func<TRequest, TEntity, Task<TEntity>> updator)
            => UpdateEntityWith((request, entity, ct) => updator(request, entity));

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> UpdateEntityWith(
            Func<TRequest, TEntity, TEntity> updator)
        {
            UpdateEntity = (request, item, entity, ct) =>
            {
                if (ct.IsCancellationRequested)
                    return Task.FromCanceled<TEntity>(ct);

                return Task.FromResult(updator((TRequest)item, entity));
            };

            return this;
        }

        public override void Build<TCompatibleRequest>(CrudRequestConfig<TCompatibleRequest> config)
        {
            base.Build(config);

            if (Selector == null)
                DefaultSelector(config);
        }

        private void DefaultSelector<TCompatibleRequest>(
            CrudRequestConfig<TCompatibleRequest> config)
        {
            var requestKey = config.GetRequestKey();
            var entityKey = config.GetKeyFor<TEntity>();

            if (requestKey != null && entityKey != null)
            {
                var builder = new SelectorBuilder<TRequest, TEntity>();
                config.SetEntitySelector<TEntity>(builder.Single(requestKey, entityKey));
            }
        }
    }
}

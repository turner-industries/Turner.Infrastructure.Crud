using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Configuration.Builders;
using Turner.Infrastructure.Crud.Requests;

namespace Turner.Infrastructure.Crud.Configuration
{
    public abstract class BulkRequestProfile<TRequest, TItem>
        : RequestProfileCommon<TRequest>
        where TRequest : ICrudRequest, IBulkRequest
    {
        private readonly Expression<Func<TRequest, IEnumerable<TItem>>> _defaultItemSource;

        public BulkRequestProfile()
        {
        }

        public BulkRequestProfile(Expression<Func<TRequest, IEnumerable<TItem>>> defaultItemSource)
        {
            _defaultItemSource = defaultItemSource;
        }

        protected BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> ForEntity<TEntity>()
            where TEntity : class
        {
            var builder = new BulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>();

            if (_defaultItemSource != null)
                builder.WithRequestItems(_defaultItemSource);

            RequestEntityBuilders[typeof(TEntity)] = builder;

            return builder;
        }
    }

    public class DefaultBulkRequestProfile<TRequest> : BulkRequestProfile<TRequest, TRequest>
        where TRequest : ICrudRequest, IBulkRequest
    {
    }
}

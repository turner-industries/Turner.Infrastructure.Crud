using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Configuration.Builders;
using Turner.Infrastructure.Crud.Requests;

namespace Turner.Infrastructure.Crud.Configuration
{
    public abstract class CrudBulkRequestProfile<TRequest, TItem>
        : CrudRequestProfileCommon<TRequest>
        where TRequest : ICrudRequest, IBulkRequest
    {
        private readonly Expression<Func<TRequest, IEnumerable<TItem>>> _defaultItemSource;

        public CrudBulkRequestProfile()
        {
        }

        public CrudBulkRequestProfile(Expression<Func<TRequest, IEnumerable<TItem>>> defaultItemSource)
        {
            _defaultItemSource = defaultItemSource;
        }

        protected CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity> ForEntity<TEntity>()
            where TEntity : class
        {
            var builder = new CrudBulkRequestEntityConfigBuilder<TRequest, TItem, TEntity>();

            if (_defaultItemSource != null)
                builder.WithRequestItems(_defaultItemSource);

            _requestEntityBuilders[typeof(TEntity)] = builder;

            return builder;
        }
    }

    public class DefaultBulkCrudRequestProfile<TRequest> : CrudBulkRequestProfile<TRequest, TRequest>
        where TRequest : ICrudRequest, IBulkRequest
    {
    }
}

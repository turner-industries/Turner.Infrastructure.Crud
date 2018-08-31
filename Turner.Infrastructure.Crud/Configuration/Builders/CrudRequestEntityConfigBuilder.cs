using System;

namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public interface ICrudRequestEntityConfigBuilder
    {
        void Build(ICrudRequestConfig config);
    }

    public class CrudRequestEntityConfigBuilder<TRequest, TEntity>
        : ICrudRequestEntityConfigBuilder
        where TEntity : class
    {
        private Func<TRequest, TEntity> _createEntityFromRequest;

        public CrudRequestEntityConfigBuilder()
        {
            _createEntityFromRequest = DefaultCreateEntity;
        }

        public CrudRequestEntityConfigBuilder<TRequest, TEntity> CreateWith(
            Func<TRequest, TEntity> creator)
        {
            _createEntityFromRequest = creator;

            return this;
        }

        public void Build(ICrudRequestConfig config)
        {
            if (!(config is CrudRequestConfig<TRequest> tConfig))
                throw new BadCrudConfigurationException();

            tConfig.SetEntityCreator(_createEntityFromRequest);
        }

        private static TEntity DefaultCreateEntity(TRequest request)
        {
            return DefaultCrudRequestConfig<TRequest>.DefaultCreateEntity<TEntity>(request);
        }
    }
}

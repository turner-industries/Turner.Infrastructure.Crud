using System.Collections.Generic;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.EntityFrameworkExtensions.Configuration;
using Z.BulkOperations;

namespace Turner.Infrastructure.Crud.EntityFrameworkExtensions.Extensions
{
    public static class BulkOperationExtensions
    {
        public static void Configure<TEntity>(this BulkOperation<TEntity> operation, BulkConfigurationType type, DataContext<TEntity> context)
            where TEntity : class
        {
            var typeConfigs = BulkConfigurationManager.Configurations(type);
            
            if (!typeConfigs.ContainsKey(context.Configuration.RequestType))
                typeConfigs[context.Configuration.RequestType] = new BulkRequestEntityConfigurationMap();

            var requestConfigs = typeConfigs[context.Configuration.RequestType];

            if (!requestConfigs.ContainsKey(typeof(TEntity)))
                requestConfigs[typeof(TEntity)] = new DefaultBulkConfiguration<TEntity>();
            
            var configurations = new List<IBulkConfiguration>();

            foreach (var tRequest in context.Configuration.RequestType.BuildTypeHierarchyDown())
            {
                if (!typeConfigs.TryGetValue(tRequest, out requestConfigs))
                    continue;

                foreach (var tEntity in typeof(TEntity).BuildTypeHierarchyDown())
                {
                    if (requestConfigs.TryGetValue(tEntity, out var entityConfigs))
                        configurations.Add(entityConfigs);
                }
            }

            foreach (var configuration in configurations)
                configuration.Apply(context.Configuration, operation);
        }
    }
}

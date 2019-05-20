using System;
using Turner.Infrastructure.Crud.Configuration.Builders;
using Turner.Infrastructure.Crud.EntityFrameworkExtensions.Configuration;

namespace Turner.Infrastructure.Crud.Configuration
{
    public static class ProfileExtensions
    {
        public static TBuilder BulkCreateWith<TRequest, TEntity, TBuilder>(
            this CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> builder,
            Func<BulkInsertConfiguration<TEntity>, BulkInsertConfiguration<TEntity>> configure)
            where TEntity : class
            where TBuilder : CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var config = configure(new BulkInsertConfiguration<TEntity>());
            if (config != null)
                BulkConfigurationManager.SetConfiguration(BulkConfigurationType.Insert, typeof(TRequest), typeof(TEntity), config);

            return (TBuilder)builder;
        }

        public static TBuilder BulkUpdateWith<TRequest, TEntity, TBuilder>(
            this CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> builder,
            Func<BulkUpdateConfiguration<TEntity>, BulkUpdateConfiguration<TEntity>> configure)
            where TEntity : class
            where TBuilder : CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var config = configure(new BulkUpdateConfiguration<TEntity>());
            if (config != null)
                BulkConfigurationManager.SetConfiguration(BulkConfigurationType.Update, typeof(TRequest), typeof(TEntity), config);

            return (TBuilder)builder;
        }

        public static TBuilder BulkDeleteWith<TRequest, TEntity, TBuilder>(
            this CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder> builder,
            Func<BulkDeleteConfiguration<TEntity>, BulkDeleteConfiguration<TEntity>> configure)
            where TEntity : class
            where TBuilder : CrudRequestEntityConfigBuilderCommon<TRequest, TEntity, TBuilder>
        {
            var config = configure(new BulkDeleteConfiguration<TEntity>());
            if (config != null)
                BulkConfigurationManager.SetConfiguration(BulkConfigurationType.Delete, typeof(TRequest), typeof(TEntity), config);

            return (TBuilder)builder;
        }
    }
}

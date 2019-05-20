namespace Turner.Infrastructure.Crud.EntityFrameworkExtensions.Configuration
{
    public class DefaultBulkConfiguration<TEntity> : BulkConfiguration<TEntity, DefaultBulkConfiguration<TEntity>>
        where TEntity : class
    {
    }
}

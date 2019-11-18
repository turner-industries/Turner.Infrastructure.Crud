using Turner.Infrastructure.Crud.EntityFrameworkCore;
using Turner.Infrastructure.Crud.EntityFrameworkExtensions;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud
{
    public static class IncludeEntityFrameworkExtensionsInitializer
    {
        public static CrudInitializer UseEntityFrameworkExtensions(this CrudInitializer initializer, 
            BulkExtensions extensions = BulkExtensions.All)
        {
            EntityFrameworkCoreInitializer.Unregister(initializer);

            return initializer
                .UseEntityFramework()
                .AddInitializer(new EntityFrameworkExtensionsInitializer(extensions));
        }
    }
}

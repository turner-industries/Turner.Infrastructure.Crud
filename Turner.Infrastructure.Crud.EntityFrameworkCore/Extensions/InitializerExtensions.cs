using Turner.Infrastructure.Crud.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud
{
    public static class IncludeEntityFrameworkCoreInitializer
    {
        public static CrudInitializer UseEntityFramework(this CrudInitializer initializer)
        {
            return initializer.AddInitializer(new EntityFrameworkCoreInitializer());
        }
    }
}

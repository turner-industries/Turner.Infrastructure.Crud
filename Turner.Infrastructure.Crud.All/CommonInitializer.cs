using Turner.Infrastructure.Crud.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud
{
    public static class CommonInitializer
    {
        public static CrudInitializer UseCommonInitialization(this CrudInitializer initializer)
        {
            return initializer
                .UseEntityFramework()
                .UseFluentValidation();
        }
    }
}

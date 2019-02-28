using Turner.Infrastructure.Crud.EntityFrameworkCore;
using Turner.Infrastructure.Crud.FluentValidation;

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

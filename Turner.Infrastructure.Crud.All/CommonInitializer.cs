using System.Reflection;
using SimpleInjector;
using Turner.Infrastructure.Crud.EntityFrameworkCore;
using Turner.Infrastructure.Crud.FluentValidation;

namespace Turner.Infrastructure.Crud.All
{
    public static partial class Crud
    {
        public static void Initialize(Container container, Assembly[] assemblies = null)
        {
            Infrastructure.Crud.Crud
                .CreateInitializer(container)
                .WithAssemblies(assemblies)
                .UseEntityFramework()
                .UseFluentValidation()
                .Initialize();
        }
    }
}

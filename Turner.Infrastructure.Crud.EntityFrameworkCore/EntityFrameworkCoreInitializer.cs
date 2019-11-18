using System.Reflection;
using SimpleInjector;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.EntityFrameworkCore
{
    public class EntityFrameworkCoreInitializer : ICrudInitializationTask
    {
        public void Run(Container container, Assembly[] assemblies, CrudOptions options)
        {
            container.Register<IEntityContext, EntityFrameworkContext>(Lifestyle.Scoped);

            var dataAgent = new EntityFrameworkDataAgent();
            container.RegisterInstance<ICreateDataAgent>(dataAgent);
            container.RegisterInstance<IUpdateDataAgent>(dataAgent);
            container.RegisterInstance<IDeleteDataAgent>(dataAgent);
            container.RegisterInstance<IBulkCreateDataAgent>(dataAgent);
            container.RegisterInstance<IBulkUpdateDataAgent>(dataAgent);
            container.RegisterInstance<IBulkDeleteDataAgent>(dataAgent);
        }

        public static void Unregister(CrudInitializer initializer)
        {
            initializer.RemoveInitializers<EntityFrameworkCoreInitializer>();
        }
    }
}

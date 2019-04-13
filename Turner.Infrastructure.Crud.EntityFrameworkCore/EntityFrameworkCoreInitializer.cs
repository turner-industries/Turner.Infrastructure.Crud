﻿using System.Reflection;
using SimpleInjector;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.EntityFrameworkCore
{
    public class EntityFrameworkCoreInitializer : ICrudInitializationTask
    {
        public void Run(Container container, Assembly[] assemblies, CrudOptions options)
        {
            container.Register<IEntityContext, EntityFrameworkContext>();
            container.Register<IDataAgent, EntityFrameworkDataAgent>();
        }
    }

    public static class IncludeInitializer
    {
        public static CrudInitializer UseEntityFramework(this CrudInitializer initializer)
        {
            return initializer.AddInitializer(new EntityFrameworkCoreInitializer());
        }
    }
}

using SimpleInjector;
using System.Collections.Generic;
using System.Reflection;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Configuration
{
    public static class SimpleInjectorCrudConfiguration
    {
        public static void ConfigureCrud(this Container container, Assembly[] assemblies)
        {
            var allAssemblies = new List<Assembly>(1 + assemblies.Length)
            {
                typeof(SimpleInjectorCrudConfiguration).Assembly
            };

            allAssemblies.AddRange(assemblies);

            var configAssemblies = allAssemblies.ToArray();

            container.RegisterSingleton(() => new CrudProfileManager(configAssemblies));


            bool IfNotHandled(PredicateContext c) => !c.Handled;

            //container.Register(typeof(GetByIdRequestHandler<,,>), configAssemblies);
            //container.RegisterConditional(typeof(IRequestHandler<,>), typeof(GetByIdRequestHandler<,,>), IfNotHandled);

            //container.Register(typeof(GetAllRequestHandler<,,,>), configAssemblies);
            //container.RegisterConditional(typeof(IRequestHandler<,>), typeof(PagedRequestHandler<,,>), IfNotHandled);
            //container.RegisterConditional(typeof(IRequestHandler<,>), typeof(GetAllRequestHandler<,,,>), IfNotHandled);

            container.Register(typeof(CreateRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(CreateRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(CreateRequestHandler<,,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(CreateRequestHandler<,,,>), IfNotHandled);

            //container.Register(typeof(EditRequestHandler<,>), configAssemblies);
            //container.Register(typeof(EditRequestHandler<,,>), configAssemblies);
            //container.RegisterConditional(typeof(IRequestHandler<>), typeof(EditRequestHandler<,>), IfNotHandled);
            //container.RegisterConditional(typeof(IRequestHandler<,>), typeof(EditRequestHandler<,,>), IfNotHandled);

            //container.Register(typeof(DeleteRequestHandler<,>), configAssemblies);
            //container.Register(typeof(DeleteRequestHandler<,,>), configAssemblies);
            //container.RegisterConditional(typeof(IRequestHandler<>), typeof(DeleteRequestHandler<,>), IfNotHandled);
            //container.RegisterConditional(typeof(IRequestHandler<,>), typeof(DeleteRequestHandler<,,>), IfNotHandled);

            //container.Register(typeof(SummaryRequestHandler<,,>), configAssemblies);
            //container.RegisterConditional(typeof(IRequestHandler<,>), typeof(SummaryRequestHandler<,,>), IfNotHandled);
        }
    }
}

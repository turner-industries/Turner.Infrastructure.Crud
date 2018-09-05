using SimpleInjector;
using System.Collections.Generic;
using System.Reflection;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Errors;
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

            container.RegisterSingleton(() => new CrudConfigManager(configAssemblies));

            bool IfNotHandled(PredicateContext c) => !c.Handled;

            container.RegisterInitializer<ICrudRequestHandler>(handler =>
            {
                if (handler.ErrorDispatcher.Handler == null)
                    handler.ErrorDispatcher.Handler = container.GetInstance<ICrudErrorHandler>();
            });

            container.Register(typeof(ICrudErrorHandler), typeof(CrudErrorHandler), Lifestyle.Singleton);

            container.RegisterConditional(typeof(IContextAccess), typeof(StandardContextAccess), IfNotHandled);
            container.RegisterConditional(typeof(IDbSetAccess), typeof(StandardDbSetAccess), IfNotHandled);

            container.RegisterConditional(typeof(ICreateAlgorithm), typeof(StandardCreateAlgorithm), IfNotHandled);
            container.Register(typeof(CreateRequestHandler<,>), configAssemblies);
            container.Register(typeof(CreateRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(CreateRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(CreateRequestHandler<,,>), IfNotHandled);

            container.RegisterConditional(typeof(IGetAlgorithm), typeof(StandardGetAlgorithm), IfNotHandled);
            container.Register(typeof(GetRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(GetRequestHandler<,,>), IfNotHandled);

            container.RegisterConditional(typeof(IGetAllAlgorithm), typeof(StandardGetAllAlgorithm), IfNotHandled);
            container.Register(typeof(GetAllRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(GetAllRequestHandler<,,>), IfNotHandled);

            container.RegisterConditional(typeof(IUpdateAlgorithm), typeof(StandardUpdateAlgorithm), IfNotHandled);
            container.Register(typeof(UpdateRequestHandler<,>), configAssemblies);
            container.Register(typeof(UpdateRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(UpdateRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(UpdateRequestHandler<,,>), IfNotHandled);

            container.RegisterConditional(typeof(IDeleteAlgorithm), typeof(StandardDeleteAlgorithm), IfNotHandled);
            container.Register(typeof(DeleteRequestHandler<,>), configAssemblies);
            container.Register(typeof(DeleteRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(DeleteRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(DeleteRequestHandler<,,>), IfNotHandled);

            container.RegisterConditional(typeof(ISaveAlgorithm), typeof(StandardSaveAlgorithm), IfNotHandled);
            container.Register(typeof(SaveRequestHandler<,>), configAssemblies);
            container.Register(typeof(SaveRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(SaveRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(SaveRequestHandler<,,>), IfNotHandled);
        }
    }
}
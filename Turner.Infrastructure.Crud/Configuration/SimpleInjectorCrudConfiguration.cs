using SimpleInjector;
using System;
using System.Linq;
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
            var allAssemblies = new Assembly[1 + assemblies.Length];
            allAssemblies[0] = typeof(SimpleInjectorCrudConfiguration).Assembly;
            Array.Copy(assemblies, 0, allAssemblies, 1, assemblies.Length);
            
            var configAssemblies = allAssemblies.Distinct().ToArray();

            container.RegisterSingleton(() => new CrudConfigManager(configAssemblies));

            bool IfNotHandled(PredicateContext c) => !c.Handled;

            container.RegisterInitializer<ICrudRequestHandler>(handler =>
            {
                if (handler.ErrorDispatcher.Handler == null)
                    handler.ErrorDispatcher.Handler = container.GetInstance<ICrudErrorHandler>();
            });

            container.Register(typeof(ICrudErrorHandler), typeof(CrudErrorHandler), Lifestyle.Singleton);
            
            container.Register(typeof(IEntityContext), typeof(EFContext));
            
            container.Register(typeof(CreateRequestHandler<,>), configAssemblies);
            container.Register(typeof(CreateRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(CreateRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(CreateRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(CreateAllRequestHandler<,>), configAssemblies);
            container.Register(typeof(CreateAllRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(CreateAllRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(CreateAllRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(GetRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(GetRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(GetAllRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(GetAllRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(PagedGetAllRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(PagedGetAllRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(PagedGetRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(PagedGetRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(PagedFindRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(PagedFindRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(UpdateRequestHandler<,>), configAssemblies);
            container.Register(typeof(UpdateRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(UpdateRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(UpdateRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(UpdateAllRequestHandler<,>), configAssemblies);
            container.Register(typeof(UpdateAllRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(UpdateAllRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(UpdateAllRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(DeleteRequestHandler<,>), configAssemblies);
            container.Register(typeof(DeleteRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(DeleteRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(DeleteRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(DeleteAllRequestHandler<,>), configAssemblies);
            container.Register(typeof(DeleteAllRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(DeleteAllRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(DeleteAllRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(SaveRequestHandler<,>), configAssemblies);
            container.Register(typeof(SaveRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(SaveRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(SaveRequestHandler<,,>), IfNotHandled);
        }
    }
}
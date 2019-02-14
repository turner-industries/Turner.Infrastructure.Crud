using SimpleInjector;
using System;
using System.Linq;
using System.Reflection;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Validation;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud
{
    public class CrudOptions
    {
        public bool ValidateAllRequests { get; set; } = false;

        public bool UseEntityFramework { get; set; } = true;

        public bool UseFluentValidation { get; set; } = true;
    }

    public static class Crud
    {
        public static void Configure(Container container, Assembly[] assemblies, CrudOptions options = null)
        {
            options = options ?? new CrudOptions();

            var allAssemblies = new Assembly[1 + assemblies.Length];
            allAssemblies[0] = typeof(Crud).Assembly;
            Array.Copy(assemblies, 0, allAssemblies, 1, assemblies.Length);

            var configAssemblies = allAssemblies.Distinct().ToArray();

            RegisterSystem(container, configAssemblies, options);
            RegisterErrorHandling(container);
            RegisterValidation(container, options);
            RegisterRequests(container, configAssemblies);
        }
        
        private static bool IfNotHandled(PredicateContext c) => !c.Handled;

        private static Predicate<DecoratorPredicateContext> ShouldValidate(CrudOptions options)
        {
            return c =>
                typeof(ICrudRequestHandler).IsAssignableFrom(c.ImplementationType) &&
                !c.ImplementationType.RequestHasAttribute(typeof(DoNotValidateAttribute)) &&
                (options.ValidateAllRequests || c.ImplementationType.RequestHasAttribute(typeof(ValidateAttribute)));
        }

        private static Predicate<DecoratorPredicateContext> ShouldMaybeValidate(CrudOptions options)
        {
            var shouldValidate = ShouldValidate(options);

            return c => typeof(ICrudRequestHandler).IsAssignableFrom(c.ImplementationType) &&
                !c.ImplementationType.RequestHasAttribute(typeof(DoNotValidateAttribute)) &&
                !shouldValidate(c) &&
                c.ImplementationType.RequestHasAttribute(typeof(MaybeValidateAttribute));
        }
        
        private static Type ValidatorFactory(DecoratorPredicateContext c)
        {
            var tRequestHandler = c.ImplementationType
                .GetInterfaces()
                .Single(x => x.IsGenericType && (
                    x.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                    x.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));

            var handlerArguments = tRequestHandler.GetGenericArguments();
            var tRequest = handlerArguments[0];

            ValidateAttribute FindAttribute(Type t)
            {
                var attr = t.GetCustomAttribute<ValidateAttribute>(false);

                if (attr == null && t.BaseType != null)
                    attr = FindAttribute(t.BaseType);

                if (attr == null)
                {
                    foreach (var x in t.GetInterfaces())
                    {
                        attr = FindAttribute(x);
                        if (attr != null) return attr;
                    }
                }

                return attr;
            }

            var validateAttribute = FindAttribute(tRequest);
            var tValidator = validateAttribute?.ValidatorType ??
                typeof(IValidator<>).MakeGenericType(tRequest);

            if (handlerArguments.Length == 1)
                return typeof(CrudValidateDecorator<,>).MakeGenericType(tRequest, tValidator);

            if (handlerArguments.Length == 2)
            {
                var tResult = handlerArguments[1];
                return typeof(CrudValidateDecorator<,,>).MakeGenericType(tRequest, tResult, tValidator);
            }

            return null;
        }

        private static void RegisterSystem(Container container, Assembly[] configAssemblies, CrudOptions options)
        {
            container.RegisterSingleton(() => new CrudConfigManager(configAssemblies));
            
            TypeRequestHookFactory.BindContainer(container.GetInstance);
            TypeEntityHookFactory.BindContainer(container.GetInstance);
            TypeItemHookFactory.BindContainer(container.GetInstance);
            TypeResultHookFactory.BindContainer(container.GetInstance);
            TypeFilterFactory.BindContainer(container.GetInstance);
            TypeSorterFactory.BindContainer(container.GetInstance);

            if (options.UseEntityFramework)
                container.Register(typeof(IEntityContext), typeof(EFContext));
        }
        
        private static void RegisterErrorHandling(Container container)
        {
            container.RegisterInitializer<ICrudRequestHandler>(handler =>
            {
                if (handler.ErrorDispatcher.Handler == null)
                    handler.ErrorDispatcher.Handler = container.GetInstance<ICrudErrorHandler>();
            });

            container.Register(typeof(ICrudErrorHandler), typeof(CrudErrorHandler), Lifestyle.Singleton);
        }

        private static void RegisterValidation(Container container, CrudOptions options)
        {
            var shouldValidate = ShouldValidate(options);
            var shouldMaybeValidate = ShouldMaybeValidate(options);

            container.RegisterDecorator(typeof(IRequestHandler<>), ValidatorFactory, Lifestyle.Transient, shouldValidate);
            container.RegisterDecorator(typeof(IRequestHandler<,>), ValidatorFactory, Lifestyle.Transient, shouldValidate);

            container.RegisterInstance(new ValidatorFactory(container.GetInstance));
            container.RegisterDecorator(typeof(IRequestHandler<>), typeof(CrudMaybeValidateDecorator<>), shouldMaybeValidate);
            container.RegisterDecorator(typeof(IRequestHandler<,>), typeof(CrudMaybeValidateDecorator<,>), shouldMaybeValidate);

            if (options.UseFluentValidation)
                container.RegisterConditional(typeof(IValidator<>), typeof(FluentValidator<>), IfNotHandled);
        }

        private static void RegisterRequests(Container container, Assembly[] configAssemblies)
        {
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

            container.Register(typeof(MergeRequestHandler<,>), configAssemblies);
            container.Register(typeof(MergeRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(MergeRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(MergeRequestHandler<,,>), IfNotHandled);

            container.Register(typeof(SynchronizeRequestHandler<,>), configAssemblies);
            container.Register(typeof(SynchronizeRequestHandler<,,>), configAssemblies);
            container.RegisterConditional(typeof(IRequestHandler<>), typeof(SynchronizeRequestHandler<,>), IfNotHandled);
            container.RegisterConditional(typeof(IRequestHandler<,>), typeof(SynchronizeRequestHandler<,,>), IfNotHandled);
        }
    }
}
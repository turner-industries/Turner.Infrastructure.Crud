using System.Reflection;
using SimpleInjector;
using Turner.Infrastructure.Crud.Validation;

namespace Turner.Infrastructure.Crud.FluentValidation
{
    public class FluentValidationInitializer : ICrudInitializationTask
    {
        private static bool IfNotHandled(PredicateContext c) => !c.Handled;

        public void Run(Container container, Assembly[] assemblies, CrudOptions options)
        {
            container.RegisterConditional(typeof(IRequestValidator<>), typeof(FluentRequestValidator<>), IfNotHandled);
        }
    }
}

using Turner.Infrastructure.Crud.FluentValidation;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud
{
    public static class IncludeInitializer
    {
        public static CrudInitializer UseFluentValidation(this CrudInitializer initializer)
        {
            return initializer.AddInitializer(new FluentValidationInitializer());
        }
    }
}

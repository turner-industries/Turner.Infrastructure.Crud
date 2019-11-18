using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Extensions
{
    internal static class StringExtensions
    {
        public static Error AsError(this string message, string propertyName = "")
            => new Error { ErrorMessage = message, PropertyName = propertyName };
    }
}

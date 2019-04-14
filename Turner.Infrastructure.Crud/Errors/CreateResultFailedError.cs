using System;
using Turner.Infrastructure.Crud.Exceptions;

namespace Turner.Infrastructure.Crud.Errors
{
    public class CreateResultFailedError : RequestFailedError
    {
        public CreateResultFailedError(object request, object item, Exception exception = null)
            : base(request, exception)
        {
            Item = item;
        }
        
        public new static bool IsReturnedFor(Exception e)
            => e is CrudCreateResultFailedException;

        public new static CreateResultFailedError From(object request, Exception exception)
        {
            if (exception is CrudCreateResultFailedException crfException)
                return new CreateResultFailedError(request, crfException.EntityProperty, crfException.InnerException);

            return new CreateResultFailedError(request, null, exception);
        }

        public object Item { get; }
    }
}

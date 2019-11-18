using System;
using Turner.Infrastructure.Crud.Exceptions;

namespace Turner.Infrastructure.Crud.Errors
{
    public class UpdateEntityFailedError : RequestFailedError
    {
        public UpdateEntityFailedError(object request, object item, object entity, Exception exception = null)
            : base(request, exception)
        {
            Item = item;
            Entity = entity;
        }

        public new static bool IsReturnedFor(Exception e)
            => e is UpdateEntityFailedException;

        public new static UpdateEntityFailedError From(object request, Exception exception)
        {
            if (exception is UpdateEntityFailedException uefException)
            {
                return new UpdateEntityFailedError(request, 
                    uefException.ItemProperty, 
                    uefException.EntityProperty,
                    uefException.InnerException);
            }

            return new UpdateEntityFailedError(request, null, null, exception);
        }

        public object Item { get; }

        public object Entity { get; }
    }
}

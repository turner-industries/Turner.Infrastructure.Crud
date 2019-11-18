using System;
using Turner.Infrastructure.Crud.Exceptions;

namespace Turner.Infrastructure.Crud.Errors
{
    public class FailedToFindError : RequestFailedError
    {
        public const string DefaultReason = "Failed to find entity.";

        public FailedToFindError(object request, Type tEntity)
            : base(request, null)
        {
            EntityType = tEntity;
            Reason = DefaultReason;
        }

        public new static bool IsReturnedFor(Exception e)
            => e is FailedToFindException;

        public new static FailedToFindError From(object request, Exception exception)
        {
            if (exception is FailedToFindException cftfException)
                return new FailedToFindError(request, cftfException.EntityTypeProperty);

            return new FailedToFindError(request, null);
        }

        public Type EntityType { get; }
    }
}

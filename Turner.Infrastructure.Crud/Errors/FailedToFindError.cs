using System;

namespace Turner.Infrastructure.Crud.Errors
{
    public class FailedToFindError : RequestFailedError
    {
        public const string DefaultReason = "Failed to find entity.";

        public FailedToFindError(object request, Type tEntity, object result = null)
            : base(request, null, result)
        {
            EntityType = tEntity;
            Reason = DefaultReason;
        }

        public Type EntityType { get; }
    }
}

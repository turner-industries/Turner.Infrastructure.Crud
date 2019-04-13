using System;
using Turner.Infrastructure.Crud.Exceptions;

namespace Turner.Infrastructure.Crud.Errors
{
    public class HookFailedError : RequestFailedError
    {
        public HookFailedError(object request, object hook, Exception exception = null, object result = null)
            : base(request, exception, result)
        {
            Hook = hook;
        }

        public new static bool IsReturnedFor(Exception e)
            => e is CrudHookFailedException;

        public new static HookFailedError From(object request, Exception exception, object result = null)
        {
            if (exception is CrudHookFailedException chfException)
                return new HookFailedError(request, chfException.HookProperty, chfException.InnerException, result);

            return new HookFailedError(request, null, exception, result);
        }

        public object Hook { get; }
    }
}

using System;

namespace Turner.Infrastructure.Crud.Errors
{
    public class RequestCanceledError : CrudError
    {
        public RequestCanceledError(object request, Exception exception = null, object result = null)
            : base(exception, result)
        {
            Request = request;
        }

        public static bool IsReturnedFor(Exception e)
            => e is OperationCanceledException;

        public static RequestCanceledError From(object request, Exception exception, object result = null)
            => new RequestCanceledError(request, exception, result);

        public object Request { get; }
    }
}

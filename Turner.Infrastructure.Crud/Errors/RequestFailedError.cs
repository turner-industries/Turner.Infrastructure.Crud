using System;
using Turner.Infrastructure.Crud.Exceptions;

namespace Turner.Infrastructure.Crud.Errors
{
    public class RequestFailedError : CrudError
    {
        public RequestFailedError(object request, Exception exception = null)
            : base(exception)
        {
            Request = request;
            Reason = exception != null ? exception.Message : string.Empty;
        }

        public static bool IsReturnedFor(Exception e)
            => e is CrudRequestFailedException
            || e is AggregateException;

        public static RequestFailedError From(object request, Exception exception)
            => new RequestFailedError(request, exception);

        public object Request { get; }

        public string Reason { get; protected set; }
    }
}

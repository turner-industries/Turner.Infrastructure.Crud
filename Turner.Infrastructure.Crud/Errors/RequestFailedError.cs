using System;

namespace Turner.Infrastructure.Crud.Errors
{
    public class RequestFailedError : CrudError
    {
        public RequestFailedError(object request, Exception exception = null, object result = null)
            : base(exception, result)
        {
            Request = request;
            Reason = exception != null ? exception.Message : string.Empty;
        }

        public object Request { get; }
        public string Reason { get; protected set; }
    }
}

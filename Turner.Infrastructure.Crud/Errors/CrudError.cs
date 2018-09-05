using System;

namespace Turner.Infrastructure.Crud.Errors
{
    public class CrudError
    {
        public CrudError(Exception exception = null, object result = null)
        {
            Exception = exception;
            Result = result;
        }

        public Exception Exception { get; }

        public object Result { get; }
    }
}

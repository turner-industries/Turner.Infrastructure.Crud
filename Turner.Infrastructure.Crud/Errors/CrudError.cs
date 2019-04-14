using System;

namespace Turner.Infrastructure.Crud.Errors
{
    public class CrudError
    {
        public CrudError(Exception exception = null)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}

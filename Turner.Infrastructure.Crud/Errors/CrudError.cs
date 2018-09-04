using System;

namespace Turner.Infrastructure.Crud.Errors
{
    public class CrudError
    {
        public Exception Exception { get; set; }
    }

    public class CrudError<TResult> : CrudError
    {
        public object Result { get; set; }
    }
}

using System;
using Turner.Infrastructure.Crud.Errors;

namespace Turner.Infrastructure.Crud.Configuration
{
    public class RequestErrorConfig
    {
        public bool? FailedToFindInGetIsError { get; set; }

        public bool? FailedToFindInGetAllIsError { get; set; }

        public bool? FailedToFindInFindIsError { get; set; }

        public bool? FailedToFindInUpdateIsError { get; set; }

        public bool? FailedToFindInDeleteIsError { get; set; }
        
        public Func<IErrorHandler> ErrorHandlerFactory { get; set; }
    }
}

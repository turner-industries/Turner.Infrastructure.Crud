using System;
using Turner.Infrastructure.Crud.Errors;

namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public class CrudRequestErrorConfig
    {
        public bool? FailedToFindInGetIsError { get; set; }

        public bool? FailedToFindInGetAllIsError { get; set; }

        public bool? FailedToFindInUpdateIsError { get; set; }

        public bool? FailedToFindInDeleteIsError { get; set; }
        
        public Func<ICrudErrorHandler> ErrorHandlerFactory { get; set; }
    }
}

using System;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public class TestErrorHandler : ErrorHandler
    {
        protected override Response HandleError(FailedToFindError error)
        {
            throw new Exception(error.Reason);
        }
    }
}

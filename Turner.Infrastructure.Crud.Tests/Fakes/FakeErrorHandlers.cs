using System;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public class TestErrorHandler : CrudErrorHandler
    {
        protected override Response HandleError(FailedToFindError error)
        {
            throw new Exception(error.Reason);
        }
    }
}

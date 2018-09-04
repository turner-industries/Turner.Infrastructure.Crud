using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public class TestErrorHandler : ICrudErrorHandler
    {
        public Response Handle(CrudError error)
            => Error.AsResponse(error.Exception.Message);

        public Response<TResult> Handle<TResult>(CrudError<TResult> error)
            => Error.AsResponse<TResult>(error.Exception.Message);
    }
}

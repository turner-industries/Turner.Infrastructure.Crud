using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Errors
{
    public class DefaultCrudErrorHandler : ICrudErrorHandler
    {
        Response ICrudErrorHandler.Handle(CrudError error)
        {
            if (error.Exception != null)
                throw error.Exception;

            // TODO: CrudError should encapsulate an error message?
            return Error.AsResponse("An error has occurred.");
        }

        public Response<TResult> Handle<TResult>(CrudError<TResult> error)
        {
            if (error.Exception != null)
                throw error.Exception;

            return Error.AsResponse<TResult>("An error has occurred.");
        }
    }
}

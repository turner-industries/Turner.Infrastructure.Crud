using System;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Errors
{
    public class ErrorDispatcher
    {
        public IErrorHandler Handler { get; internal set; }

        public ErrorDispatcher(IErrorHandler handler)
        {
            Handler = handler;
        }

        public Response Dispatch(CrudError error)
        {
            return Handler.Handle(error);
        }

        public Response<TResult> Dispatch<TResult>(CrudError error)
        {
            return Handler.Handle<TResult>(error);
        }

        public Response Dispatch(Exception exception)
        {
            return Handler.Handle(new CrudError(exception));
        }

        public Response<TResult> Dispatch<TResult>(Exception exception)
        {
            return Handler.Handle<TResult>(new CrudError(exception));
        }
    }
}
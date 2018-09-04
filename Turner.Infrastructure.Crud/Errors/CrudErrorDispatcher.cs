using System;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Errors
{
    public class CrudErrorDispatcher
    {
        public ICrudErrorHandler Handler { get; internal set; }

        public CrudErrorDispatcher(ICrudErrorHandler handler)
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

        public Response<TResult> Dispatch<TResult>(Exception exception, TResult result = default(TResult))
        {
            return Handler.Handle<TResult>(new CrudError(exception, result));
        }
    }
}

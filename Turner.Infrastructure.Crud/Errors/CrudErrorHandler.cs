using System;
using System.Collections.Generic;
using System.Linq;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Errors
{
    public interface ICrudErrorHandler
    {
        Response Handle(CrudError error);

        Response<TResult> Handle<TResult>(CrudError error);
    }

    public class CrudErrorHandler : ICrudErrorHandler
    {
        public const string GenericErrorMessage = "An error has occurred.";

        private readonly Dictionary<Type, Func<CrudError, Response>> _dispatchers;

        public CrudErrorHandler()
        {
            _dispatchers = new Dictionary<Type, Func<CrudError, Response>>
            {
                { typeof(RequestFailedError), e => HandleError((RequestFailedError) e) },
                { typeof(FailedToFindError), e => HandleError((FailedToFindError) e) }
            };
        }

        public Response Handle(CrudError error)
        {
            if (_dispatchers.TryGetValue(error.GetType(), out var dispatcher))
                return dispatcher(error);

            return HandleError(error);
        }

        public Response<TResult> Handle<TResult>(CrudError error)
        {
            return new Response<TResult>
            {
                Errors = Handle(error).Errors.ToList(),
                Data = (TResult) error.Result
            };
        }

        protected virtual Response HandleError(CrudError error)
        {
            if (error.Exception != null)
                throw error.Exception;

            return Error.AsResponse(GenericErrorMessage);
        }
        
        protected virtual Response HandleError(RequestFailedError error)
        {
            return Error.AsResponse(error.Reason);
        }
        
        protected virtual Response HandleError(FailedToFindError error)
        {
            return Error.AsResponse(error.Reason);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Errors
{
    public interface IErrorHandler
    {
        Response Handle(CrudError error);

        Response<TResult> Handle<TResult>(CrudError error);
    }

    public class ErrorHandler : IErrorHandler
    {
        public const string GenericErrorMessage = "An error occurred while processing the request.";
        public const string CanceledErrorMessage = "The request was canceled while processing.";

        private readonly Dictionary<Type, Func<CrudError, Response>> _dispatchers;

        public ErrorHandler()
        {
            _dispatchers = new Dictionary<Type, Func<CrudError, Response>>
            {
                { typeof(RequestFailedError), e => HandleError((RequestFailedError)e) },
                { typeof(FailedToFindError), e => HandleError((FailedToFindError)e) },
                { typeof(RequestCanceledError), e => HandleError((RequestCanceledError)e) },
                { typeof(HookFailedError), e => HandleError((HookFailedError)e) },
                { typeof(CreateEntityFailedError), e => HandleError((CreateEntityFailedError)e) },
                { typeof(UpdateEntityFailedError), e => HandleError((UpdateEntityFailedError)e) },
                { typeof(CreateResultFailedError), e => HandleError((CreateResultFailedError)e) }
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
                Data = default(TResult)
            };
        }

        protected virtual Response HandleError(CrudError error)
        {
            if (error.Exception != null)
                throw error.Exception;

            return GenericErrorMessage.AsError().AsResponse();
        }
        
        protected virtual Response HandleError(RequestFailedError error)
            => error.Reason.AsError().AsResponse();
        
        protected virtual Response HandleError(RequestCanceledError error)
            => CanceledErrorMessage.AsError().AsResponse();

        protected virtual Response HandleError(FailedToFindError error)
            => error.Reason.AsError().AsResponse();

        protected virtual Response HandleError(HookFailedError error)
            => error.Reason.AsError().AsResponse();

        protected virtual Response HandleError(CreateEntityFailedError error)
            => error.Reason.AsError().AsResponse();

        protected virtual Response HandleError(UpdateEntityFailedError error)
            => error.Reason.AsError().AsResponse();

        protected virtual Response HandleError(CreateResultFailedError error)
            => error.Reason.AsError().AsResponse();
    }
}

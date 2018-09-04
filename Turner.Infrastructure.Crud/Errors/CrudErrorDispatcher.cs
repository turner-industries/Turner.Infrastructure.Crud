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

        public Response Dispatch(CrudRequestFailedException requestFailedException)
        {
            var error = new CrudError { Exception = requestFailedException };

            return Handler.Handle(error);
        }

        public Response<TResult> Dispatch<TResult>(CrudRequestFailedException requestFailedException)
        {
            var error = new CrudError<TResult>
            {
                Exception = requestFailedException,
                Result = requestFailedException.ResponseData
            };

            return Handler.Handle(error);
        }
    }
}

using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Errors
{
    public interface ICrudErrorHandler
    {
        Response Handle(CrudError error);
        Response<TResult> Handle<TResult>(CrudError<TResult> error);
    }
}

using Turner.Infrastructure.Crud.Errors;

namespace Turner.Infrastructure.Crud.Requests
{
    internal interface ICrudRequestHandler
    {
        CrudErrorDispatcher ErrorDispatcher { get; }
    }
}

using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;

namespace Turner.Infrastructure.Crud.Requests
{
    internal interface ICrudRequestHandler
    {
        CrudErrorDispatcher ErrorDispatcher { get; }
    }

    internal abstract class CrudRequestHandler<TRequest, TEntity> : ICrudRequestHandler
        where TEntity : class
    {
        protected readonly IEntityContext Context;
        protected readonly ICrudRequestConfig RequestConfig;
        
        protected CrudRequestHandler(IEntityContext context, CrudConfigManager profileManager)
        {
            Context = context;
            RequestConfig = profileManager.GetRequestConfigFor<TRequest>();

            var errorHandler = RequestConfig.ErrorConfig.GetErrorHandlerFor<TEntity>();
            ErrorDispatcher = new CrudErrorDispatcher(errorHandler);
        }

        public CrudErrorDispatcher ErrorDispatcher { get; }
    }
}

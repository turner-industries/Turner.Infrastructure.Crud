using Microsoft.EntityFrameworkCore;
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
        protected readonly DbContext Context;
        protected readonly ICrudRequestConfig RequestConfig;
        
        protected CrudRequestHandler(DbContext context, CrudConfigManager profileManager)
        {
            Context = context;
            RequestConfig = profileManager.GetRequestConfigFor<TRequest>();

            var errorHandler = RequestConfig.ErrorConfig.GetErrorHandlerFor<TEntity>();
            ErrorDispatcher = new CrudErrorDispatcher(errorHandler);
        }

        public CrudErrorDispatcher ErrorDispatcher { get; private set; }
    }
}

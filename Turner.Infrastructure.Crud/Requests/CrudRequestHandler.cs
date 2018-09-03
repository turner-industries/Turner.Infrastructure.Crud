using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Crud.Configuration;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class CrudRequestHandler<TRequest>
    {
        protected readonly DbContext Context;
        protected readonly ICrudRequestConfig RequestConfig;

        protected CrudRequestHandler(DbContext context, CrudConfigManager profileManager)
        {
            Context = context;
            RequestConfig = profileManager.GetRequestConfigFor<TRequest>();
        }
    }
}

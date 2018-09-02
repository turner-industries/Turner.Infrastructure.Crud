using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    public abstract class CreateRequestHandlerBase<TRequest, TEntity>
        where TEntity : class
    {
        protected readonly DbContext Context;
        protected readonly ICrudRequestConfig RequestConfig;

        public CreateRequestHandlerBase(DbContext context, CrudConfigManager profileManager)
        {
            Context = context;
            RequestConfig = profileManager.GetRequestConfigFor<TRequest>();
        }

        protected async Task<TEntity> CreateEntity(TRequest request)
        {
            await RequestConfig.PreCreate<TEntity>(request);

            var entity = RequestConfig.CreateEntity<TEntity>(request);
            await Context.Set<TEntity>().AddAsync(entity);

            await RequestConfig.PostCreate(entity);
            await Context.SaveChangesAsync();

            return entity;
        }
    }

    public class CreateRequestHandler<TRequest, TEntity>
        : CreateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : ICreateRequest<TEntity>
    {
        public CreateRequestHandler(DbContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            await CreateEntity(request);

            return Response.Success();
        }
    }

    public class CreateRequestHandler<TRequest, TEntity, TOut>
        : CreateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : ICreateRequest<TEntity, TOut>
    {
        public CreateRequestHandler(DbContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            var entity = await CreateEntity(request);
            var result = Mapper.Map<TOut>(entity);

            return new Response<TOut> { Data = result };
        }
    }
}

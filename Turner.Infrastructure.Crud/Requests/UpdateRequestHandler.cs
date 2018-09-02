using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class UpdateRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest>
        where TEntity : class
    {
        public UpdateRequestHandlerBase(DbContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        protected async Task<TEntity> GetEntity(TRequest request)
        {
            var selector = RequestConfig.UpdateSelector<TEntity>();
            var entity = await Context.Set<TEntity>()
                .SelectAsync(request, selector);
            
            if (entity == null && RequestConfig.FailedToFindInUpdateIsError)
            {
                throw new FailedToFindException("Failed to find entity.")
                {
                    RequestTypeProperty = request.GetType(),
                    QueryTypeProperty = typeof(TEntity)
                };
            }

            return entity;
        }

        protected async Task UpdateEntity(TRequest request, TEntity entity)
        {
            await RequestConfig.PreUpdate<TEntity>(request);

            await RequestConfig.UpdateEntity(request, entity);

            await RequestConfig.PostUpdate(entity);

            await Context.SaveChangesAsync();
        }
    }

    internal class UpdateRequestHandler<TRequest, TEntity>
        : UpdateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : IUpdateRequest<TEntity>
    {
        public UpdateRequestHandler(DbContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            var entity = await GetEntity(request);
            if (entity != null)
                await UpdateEntity(request, entity);

            return Response.Success();
        }
    }

    internal class UpdateRequestHandler<TRequest, TEntity, TOut>
        : UpdateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : IUpdateRequest<TEntity, TOut>
    {
        public UpdateRequestHandler(DbContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            var entity = await GetEntity(request);
            TOut result = default(TOut);

            if (entity != null)
            {
                await UpdateEntity(request, entity);
                result = Mapper.Map<TOut>(entity);
            }

            return new Response<TOut> { Data = result };
        }
    }
}

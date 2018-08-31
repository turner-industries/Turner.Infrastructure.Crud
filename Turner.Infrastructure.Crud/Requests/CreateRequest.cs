using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface ICreateRequest<TEntity> : IRequest
        where TEntity : class
    {       
    }

    public interface ICreateRequest<TEntity, TOutput> : IRequest<TOutput>
        where TEntity : class
    {
    }

    [DoNotValidate]
    public class CreateRequest<TEntity, TInput> : ICreateRequest<TEntity>
        where TEntity : class
    {
        public TInput Data { get; }
        
        public CreateRequest(TInput data)
        {
            Data = data;
        }
    }

    [DoNotValidate]
    public class CreateRequest<TEntity, TInput, TOutput> : ICreateRequest<TEntity, TOutput>
        where TEntity : class
    {
        public TInput Data { get; }
        
        public CreateRequest(TInput data)
        {
            Data = data;
        }
    }

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
            var entity = RequestConfig.CreateEntity<TEntity>(request);
            await Context.Set<TEntity>().AddAsync(entity);
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

    public class CreateRequestHandler<TRequest, TEntity, TOutput>
        : CreateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOutput>
        where TEntity : class
        where TRequest : ICreateRequest<TEntity, TOutput>
    {
        public CreateRequestHandler(DbContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<TOutput>> HandleAsync(TRequest request)
        {
            var entity = await CreateEntity(request);
            var result = Mapper.Map<TOutput>(entity);

            return new Response<TOutput> { Data = result };
        }
    }
}

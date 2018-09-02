using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface ICreateAlgorithm
    {
        Task<TEntity> CreateEntityAsync<TEntity>(DbContext context, TEntity entity)
            where TEntity : class;

        Task SaveChangesAsync(DbContext context);
    }

    public class StandardCreateAlgorithm : ICreateAlgorithm
    {
        private readonly IContextAccess _contextAccess;
        private readonly IDbSetAccess _setAccess;

        public StandardCreateAlgorithm(IContextAccess contextAccess, 
            IDbSetAccess setAccess)
        {
            _contextAccess = contextAccess;
            _setAccess = setAccess;
        }

        public Task<TEntity> CreateEntityAsync<TEntity>(DbContext context, TEntity entity)
            where TEntity : class
        {
            var set = _contextAccess.GetEntities<TEntity>(context);
            return _setAccess.CreateAsync(entity, set);
        }

        public Task SaveChangesAsync(DbContext context)
        {
            return _contextAccess.ApplyChangesAsync(context);
        }
    }

    internal abstract class CreateRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest>
        where TEntity : class
    {
        protected readonly ICreateAlgorithm Algorithm;

        public CreateRequestHandlerBase(DbContext context, 
            CrudConfigManager profileManager,
            ICreateAlgorithm algorithm)
            : base(context, profileManager)
        {
            Algorithm = algorithm;
        }

        protected async Task<TEntity> CreateEntity(TRequest request)
        {
            await RequestConfig.PreCreate<TEntity>(request).Configure();

            var entity = await RequestConfig.CreateEntity<TEntity>(request).Configure();
            var newEntity = await Algorithm.CreateEntityAsync(Context, entity).Configure();

            await RequestConfig.PostCreate(entity).Configure();
            await Algorithm.SaveChangesAsync(Context).Configure();

            return entity;
        }
    }

    internal class CreateRequestHandler<TRequest, TEntity>
        : CreateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : ICreateRequest<TEntity>
    {
        public CreateRequestHandler(DbContext context, 
            CrudConfigManager profileManager,
            ICreateAlgorithm algorithm)
            : base(context, profileManager, algorithm)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            await CreateEntity(request).Configure();

            return Response.Success();
        }
    }

    internal class CreateRequestHandler<TRequest, TEntity, TOut>
        : CreateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : ICreateRequest<TEntity, TOut>
    {
        public CreateRequestHandler(DbContext context, 
            CrudConfigManager profileManager,
            ICreateAlgorithm algorithm)
            : base(context, profileManager, algorithm)
        {
        }

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            var entity = await CreateEntity(request).Configure();
            var result = Mapper.Map<TOut>(entity);

            return new Response<TOut> { Data = result };
        }
    }
}

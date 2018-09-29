using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface ISaveAlgorithm
    {
        DbSet<TEntity> GetEntities<TEntity>(DbContext context)
            where TEntity : class;
        
        Task<TEntity> CreateEntityAsync<TEntity>(DbContext context, TEntity entity)
            where TEntity : class;

        Task SaveCreateChangesAsync(DbContext context);

        Task SaveUpdateChangesAsync(DbContext context);
    }

    public class StandardSaveAlgorithm : ISaveAlgorithm
    {
        private readonly ICreateAlgorithm _createAlgorithm;
        private readonly IUpdateAlgorithm _updateAlgorithm;

        public StandardSaveAlgorithm(ICreateAlgorithm createAlgorithm, IUpdateAlgorithm updateAlgorithm)
        {
            _createAlgorithm = createAlgorithm;
            _updateAlgorithm = updateAlgorithm;
        }

        public DbSet<TEntity> GetEntities<TEntity>(DbContext context)
            where TEntity : class
        {
            return _updateAlgorithm.GetEntities<TEntity>(context);
        }

        public Task<TEntity> CreateEntityAsync<TEntity>(DbContext context, TEntity entity)
            where TEntity : class
        {
            return _createAlgorithm.CreateEntityAsync(context, entity);
        }

        public Task SaveUpdateChangesAsync(DbContext context)
        {
            return _updateAlgorithm.SaveChangesAsync(context);
        }

        public Task SaveCreateChangesAsync(DbContext context)
        {
            return _createAlgorithm.SaveChangesAsync(context);
        }
    }

    internal abstract class SaveRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected readonly ISaveAlgorithm Algorithm;
        protected readonly RequestOptions Options;

        protected SaveRequestHandlerBase(DbContext context,
            CrudConfigManager profileManager,
            ISaveAlgorithm algorithm)
            : base(context, profileManager)
        {
            Algorithm = algorithm;
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        protected async Task<TEntity> GetEntity(TRequest request)
        {
            var selector = RequestConfig.GetSelectorFor<TEntity>(SelectorType.Update);
            var entity = await Algorithm.GetEntities<TEntity>(Context)
                .SelectSingleAsync(request, selector)
                .Configure();
            
            return entity;
        }

        protected async Task<TEntity> SaveEntity(TRequest request, TEntity entity)
        {
            await RequestConfig.RunPreActionsFor<TEntity>(ActionType.Save, request).Configure();

            if (entity == null)
            {
                entity = await CreateEntity(request).Configure();
                await RequestConfig.RunPostActionsFor(ActionType.Save, request, entity).Configure();
                await Algorithm.SaveCreateChangesAsync(Context).Configure();
            }
            else
            {
                await UpdateEntity(request, entity).Configure();
                await RequestConfig.RunPostActionsFor(ActionType.Save, request, entity).Configure();
                await Algorithm.SaveUpdateChangesAsync(Context).Configure();
            }

            return entity;
        }

        private async Task<TEntity> CreateEntity(TRequest request)
        {
            if (!Options.SuppressCreateActionsInSave)
            {
                await RequestConfig
                    .RunPreActionsFor<TEntity>(ActionType.Create, request)
                    .Configure();
            }

            var entity = await RequestConfig.CreateEntity<TEntity>(request).Configure();
            var newEntity = await Algorithm.CreateEntityAsync(Context, entity).Configure();

            if (!Options.SuppressCreateActionsInSave)
                await RequestConfig.RunPostActionsFor(ActionType.Create, request, newEntity).Configure();
            
            return entity;
        }

        private async Task UpdateEntity(TRequest request, TEntity entity)
        {
            if (!Options.SuppressUpdateActionsInSave)
            {
                await RequestConfig
                    .RunPreActionsFor<TEntity>(ActionType.Update, request)
                    .Configure();
            }

            await RequestConfig.UpdateEntity(request, entity).Configure();

            if (!Options.SuppressUpdateActionsInSave)
                await RequestConfig.RunPostActionsFor(ActionType.Update, request, entity).Configure();
        }
    }

    internal class SaveRequestHandler<TRequest, TEntity>
        : SaveRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : ISaveRequest<TEntity>
    {
        public SaveRequestHandler(DbContext context,
            CrudConfigManager profileManager,
            ISaveAlgorithm algorithm)
            : base(context, profileManager, algorithm)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            TEntity entity;

            try
            {
                entity = await GetEntity(request).Configure();
            }
            catch (CrudRequestFailedException e)
            {
                var error = new RequestFailedError(request, e);
                return ErrorDispatcher.Dispatch(error);
            }

            await SaveEntity(request, entity).Configure();

            return Response.Success();
        }
    }

    internal class SaveRequestHandler<TRequest, TEntity, TOut>
        : SaveRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : ISaveRequest<TEntity, TOut>
    {
        public SaveRequestHandler(DbContext context,
            CrudConfigManager profileManager,
            ISaveAlgorithm algorithm)
            : base(context, profileManager, algorithm)
        {
        }

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            TEntity entity;

            try
            {
                entity = await GetEntity(request).Configure();
            }
            catch (CrudRequestFailedException e)
            {
                var error = new RequestFailedError(request, e);
                return ErrorDispatcher.Dispatch<TOut>(error);
            }

            var newEntity = await SaveEntity(request, entity).Configure();
            var result = Mapper.Map<TOut>(newEntity);

            return result.AsResponse();
        }
    }
}

using AutoMapper;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    //internal abstract class MergeRequestHandlerBase<TRequest, TEntity>
    //    : CrudRequestHandler<TRequest, TEntity>
    //    where TEntity : class
    //{
    //    protected readonly RequestOptions Options;

    //    protected MergeRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
    //        : base(context, profileManager)
    //    {
    //        Options = RequestConfig.GetOptionsFor<TEntity>();
    //    }

    //    protected Task<TEntity> GetEntity(TRequest request)
    //    {
    //        var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
    //        var set = Context.EntitySet<TEntity>();

    //        return Context.SingleOrDefaultAsync(set, selector(request));
    //    }

    //    protected async Task<TEntity[]> GetExistingEntities(TRequest request)
    //    {
    //        var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
    //        var entities = Context.EntitySet<TEntity>().AsQueryable().Where(selector(request));
            
    //        return await Context.ToArrayAsync(entities).Configure();
    //    }

    //    protected TEntity[] GetMissingEntities(TRequest request, TEntity[] existing)
    //    {
    //        var selector = RequestConfig.GetSelectorFor<TEntity>().Get<TEntity>();
    //        //var entities = 

    //        return null;
    //    }

    //    protected async Task<TEntity> SaveEntity(TRequest request, TEntity entity)
    //    {
    //        await RequestConfig.RunPreActionsFor<TEntity>(ActionType.Save, request).Configure();

    //        if (entity == null)
    //        {
    //            entity = await CreateEntities(request).Configure();
    //            await RequestConfig.RunPostActionsFor(ActionType.Save, request, entity).Configure();
    //            await Context.ApplyChangesAsync().Configure();
    //        }
    //        else
    //        {
    //            entity = await UpdateEntities(request, entity).Configure();
    //            await RequestConfig.RunPostActionsFor(ActionType.Save, request, entity).Configure();
    //            await Context.ApplyChangesAsync().Configure();
    //        }

    //        return entity;
    //    }

    //    private async Task<TEntity> CreateEntities(TRequest request)
    //    {
    //        if (!Options.SuppressCreateActionsInSave)
    //        {
    //            await RequestConfig
    //                .RunPreActionsFor<TEntity>(ActionType.Create, request)
    //                .Configure();
    //        }

    //        var entity = await RequestConfig.CreateEntity<TEntity>(request).Configure();
    //        entity = await Context.EntitySet<TEntity>().CreateAsync(entity).Configure();

    //        if (!Options.SuppressCreateActionsInSave)
    //            await RequestConfig.RunPostActionsFor(ActionType.Create, request, entity).Configure();

    //        return entity;
    //    }

    //    private async Task<TEntity> UpdateEntity(TRequest request, TEntity entity)
    //    {
    //        if (!Options.SuppressUpdateActionsInSave)
    //        {
    //            await RequestConfig
    //                .RunPreActionsFor<TEntity>(ActionType.Update, request)
    //                .Configure();
    //        }

    //        await RequestConfig.UpdateEntity(request, entity).Configure();
    //        entity = await Context.EntitySet<TEntity>().UpdateAsync(entity).Configure();

    //        if (!Options.SuppressUpdateActionsInSave)
    //            await RequestConfig.RunPostActionsFor(ActionType.Update, request, entity).Configure();

    //        return entity;
    //    }
    //}

    //internal class SaveRequestHandler<TRequest, TEntity>
    //    : SaveRequestHandlerBase<TRequest, TEntity>,
    //      IRequestHandler<TRequest>
    //    where TEntity : class
    //    where TRequest : ISaveRequest<TEntity>
    //{
    //    public SaveRequestHandler(IEntityContext context, CrudConfigManager profileManager)
    //        : base(context, profileManager)
    //    {
    //    }

    //    public async Task<Response> HandleAsync(TRequest request)
    //    {
    //        TEntity entity;

    //        try
    //        {
    //            entity = await GetEntity(request).Configure();
    //        }
    //        catch (CrudRequestFailedException e)
    //        {
    //            var error = new RequestFailedError(request, e);
    //            return ErrorDispatcher.Dispatch(error);
    //        }

    //        await SaveEntity(request, entity).Configure();

    //        return Response.Success();
    //    }
    //}

    //internal class SaveRequestHandler<TRequest, TEntity, TOut>
    //    : SaveRequestHandlerBase<TRequest, TEntity>,
    //      IRequestHandler<TRequest, TOut>
    //    where TEntity : class
    //    where TRequest : ISaveRequest<TEntity, TOut>
    //{
    //    public SaveRequestHandler(IEntityContext context, CrudConfigManager profileManager)
    //        : base(context, profileManager)
    //    {
    //    }

    //    public async Task<Response<TOut>> HandleAsync(TRequest request)
    //    {
    //        TEntity entity;

    //        try
    //        {
    //            entity = await GetEntity(request).Configure();
    //        }
    //        catch (CrudRequestFailedException e)
    //        {
    //            var error = new RequestFailedError(request, e);
    //            return ErrorDispatcher.Dispatch<TOut>(error);
    //        }

    //        var newEntity = await SaveEntity(request, entity).Configure();
    //        var result = Mapper.Map<TOut>(newEntity);

    //        return result.AsResponse();
    //    }
    //}
}

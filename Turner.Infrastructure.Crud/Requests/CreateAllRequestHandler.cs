using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal abstract class CreateAllRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
        where TRequest : ICreateAllRequest
    {
        protected CreateAllRequestHandlerBase(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        protected async Task<TEntity[]> CreateEntities(TRequest request, CancellationToken ct)
        {
            await request.RunRequestHooks(RequestConfig, ct).Configure();
            
            var itemSource = RequestConfig.GetRequestItemSourceFor<TEntity>();
            var items = ((IEnumerable<object>)itemSource.ItemSource(request)).ToArray();

            items = await request.RunItemHooks<TEntity>(RequestConfig, items, ct).Configure();
            var entities = await request.CreateEntities<TEntity>(RequestConfig, items, ct).Configure();

            entities = await Context.Set<TEntity>().CreateAsync(entities, ct).Configure();
            ct.ThrowIfCancellationRequested();
            
            await request.RunEntityHooks<TEntity>(RequestConfig, entities, ct).Configure();
            
            await Context.ApplyChangesAsync(ct).Configure();
            ct.ThrowIfCancellationRequested();

            return entities;
        }
    }

    internal class CreateAllRequestHandler<TRequest, TEntity>
        : CreateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : ICreateAllRequest<TEntity>
    {
        public CreateAllRequestHandler(IEntityContext context,
            CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                try
                {
                    await CreateEntities(request, ct).Configure();
                }
                catch (Exception e) when (RequestFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch(RequestFailedError.From(request, e));
                }
                catch (Exception e) when (RequestCanceledError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch(RequestCanceledError.From(request, e));
                }
                catch (Exception e) when (HookFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch(HookFailedError.From(request, e));
                }
            }

            return Response.Success();
        }
    }

    internal class CreateAllRequestHandler<TRequest, TEntity, TOut>
        : CreateAllRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, CreateAllResult<TOut>>
        where TEntity : class
        where TRequest : ICreateAllRequest<TEntity, TOut>
    {
        public CreateAllRequestHandler(IEntityContext context,
            CrudConfigManager profileManager)
            : base(context, profileManager)
        {
        }

        public async Task<Response<CreateAllResult<TOut>>> HandleAsync(TRequest request)
        {
            CreateAllResult<TOut> result = null;

            using (var cts = new CancellationTokenSource())
            {
                var ct = cts.Token;

                try
                {
                    var entities = await CreateEntities(request, ct).Configure();
                    var tOuts = await entities.CreateResults<TEntity, TOut>(RequestConfig, ct).Configure();
                    var items = await request.RunResultHooks(RequestConfig, tOuts, ct).Configure();

                    result = new CreateAllResult<TOut>(items);
                }
                catch (Exception e) when (RequestFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<CreateAllResult<TOut>>(RequestFailedError.From(request, e, result));
                }
                catch (Exception e) when (RequestCanceledError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<CreateAllResult<TOut>>(RequestCanceledError.From(request, e, result));
                }
                catch (Exception e) when (HookFailedError.IsReturnedFor(e))
                {
                    return ErrorDispatcher.Dispatch<CreateAllResult<TOut>>(HookFailedError.From(request, e, result));
                }
            }

            return result.AsResponse();
        }
    }
}

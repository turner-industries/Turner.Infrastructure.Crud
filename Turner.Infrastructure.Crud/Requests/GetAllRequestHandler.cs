using System;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal class GetAllRequestHandler<TRequest, TEntity, TOut>
        : CrudRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, GetAllResult<TOut>>
        where TEntity : class
        where TRequest : IGetAllRequest<TEntity, TOut>
    {
        protected readonly RequestOptions Options;

        public GetAllRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public Task<Response<GetAllResult<TOut>>> HandleAsync(TRequest request)
        {
            return HandleWithErrorsAsync(request, HandleAsync);
        }

        private async Task<GetAllResult<TOut>> HandleAsync(TRequest request, CancellationToken token)
        {
            await request.RunRequestHooks(RequestConfig, token).Configure();

            var entities = Context.Set<TEntity>()
                .FilterWith(request, RequestConfig)
                .SortWith(request, RequestConfig);
            
            var items = Array.Empty<TOut>();

            if (Options.UseProjection)
            {
                items = await entities.ProjectToArrayAsync<TEntity, TOut>(token).Configure();
                token.ThrowIfCancellationRequested();
                
                if (items.Length == 0)
                {
                    if (RequestConfig.ErrorConfig.FailedToFindInGetAllIsError)
                        throw new CrudFailedToFindException { EntityTypeProperty = typeof(TEntity) };

                    var defaultEntity = RequestConfig.GetDefaultFor<TEntity>();
                    if (defaultEntity != null)
                    {
                        items = new TOut[] 
                        {
                            await defaultEntity.CreateResult<TEntity, TOut>(RequestConfig, token).Configure()
                        };
                    }
                }
            }
            else
            {
                var resultEntities = await entities.ToArrayAsync(token).Configure();
                token.ThrowIfCancellationRequested();

                if (resultEntities.Length == 0)
                {
                    if (RequestConfig.ErrorConfig.FailedToFindInGetAllIsError)
                        throw new CrudFailedToFindException { EntityTypeProperty = typeof(TEntity) };

                    var defaultEntity = RequestConfig.GetDefaultFor<TEntity>();
                    if (defaultEntity != null)
                        resultEntities = new TEntity[] { RequestConfig.GetDefaultFor<TEntity>() };
                }

                await request.RunEntityHooks<TEntity>(RequestConfig, entities, token).Configure();

                items = await resultEntities.CreateResults<TEntity, TOut>(RequestConfig, token).Configure();
            }

            token.ThrowIfCancellationRequested();

            var result = new GetAllResult<TOut>(items); 

            return await request.RunResultHooks(RequestConfig, result, token).Configure();
        }
    }
}

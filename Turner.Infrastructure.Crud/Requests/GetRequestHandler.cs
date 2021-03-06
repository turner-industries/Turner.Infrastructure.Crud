﻿using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal class GetRequestHandler<TRequest, TEntity, TOut>
        : CrudRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : IGetRequest<TEntity, TOut>
    {
        protected readonly RequestOptions Options;

        public GetRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public Task<Response<TOut>> HandleAsync(TRequest request)
        {
            return HandleWithErrorsAsync(request, HandleAsync);
        }

        private async Task<TOut> HandleAsync(TRequest request, CancellationToken token)
        {
            await request.RunRequestHooks(RequestConfig, token).Configure();

            var entities = Context.Set<TEntity>()
                .FilterWith(request, RequestConfig)
                .SelectWith(request, RequestConfig);

            var result = default(TOut);

            if (Options.UseProjection)
            {
                result = await entities.ProjectSingleOrDefaultAsync<TEntity, TOut>(token).Configure();
                token.ThrowIfCancellationRequested();
                
                if (result == null)
                {
                    if (RequestConfig.ErrorConfig.FailedToFindInGetIsError)
                        throw new CrudFailedToFindException { EntityTypeProperty = typeof(TEntity) };

                    result = await RequestConfig.GetDefaultFor<TEntity>()
                        .CreateResult<TEntity, TOut>(RequestConfig, token)
                        .Configure();
                }
            }
            else
            {
                var entity = await entities.SingleOrDefaultAsync(token).Configure();
                token.ThrowIfCancellationRequested();

                if (entity == null)
                {
                    if (RequestConfig.ErrorConfig.FailedToFindInGetIsError)
                        throw new CrudFailedToFindException { EntityTypeProperty = typeof(TEntity) };

                    entity = RequestConfig.GetDefaultFor<TEntity>();
                }

                await request.RunEntityHooks<TEntity>(RequestConfig, entity, token).Configure();

                result = await entity
                    .CreateResult<TEntity, TOut>(RequestConfig, token)
                    .Configure();
            }

            token.ThrowIfCancellationRequested();
            
            return await request.RunResultHooks(RequestConfig, result, token).Configure();
        }
    }
}

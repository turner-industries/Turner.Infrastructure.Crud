﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Extensions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    internal class PagedGetAllRequestHandler<TRequest, TEntity, TOut>
        : CrudRequestHandler<TRequest, TEntity>, IRequestHandler<TRequest, PagedGetAllResult<TOut>>
        where TEntity : class
        where TRequest : IPagedGetAllRequest<TEntity, TOut>
    {
        protected readonly RequestOptions Options;

        public PagedGetAllRequestHandler(IEntityContext context, CrudConfigManager profileManager)
            : base(context, profileManager)
        {
            Options = RequestConfig.GetOptionsFor<TEntity>();
        }

        public Task<Response<PagedGetAllResult<TOut>>> HandleAsync(TRequest request)
        {
            return HandleWithErrorsAsync(request, HandleAsync);
        }

        public async Task<PagedGetAllResult<TOut>> HandleAsync(TRequest request, CancellationToken token)
        {
            await request.RunRequestHooks(RequestConfig, token).Configure();
                    
            var entities = Context
                .Set<TEntity>()
                .FilterWith(request, RequestConfig)
                .SortWith(request, RequestConfig);
                    
            var totalItemCount = await entities.CountAsync(token).Configure();
            token.ThrowIfCancellationRequested();

            var pageSize = request.PageSize < 1 ? totalItemCount : request.PageSize;
            var totalPageCount = totalItemCount == 0 ? 1 : (totalItemCount + pageSize - 1) / pageSize;
            var pageNumber = Math.Max(1, Math.Min(request.PageNumber, totalPageCount));
            var startIndex = (pageNumber - 1) * pageSize;

            entities = entities.Skip(startIndex).Take(pageSize);

            var items = await GetItems(request, entities, token).Configure();
                    
            return new PagedGetAllResult<TOut>(items, pageNumber, pageSize, totalPageCount, totalItemCount);
        }

        private async Task<TOut[]> GetItems(TRequest request, IQueryable<TEntity> entities, CancellationToken token)
        {
            var tOuts = Array.Empty<TOut>();

            if (Options.UseProjection)
            {
                tOuts = await entities.ProjectToArrayAsync<TEntity, TOut>(token).Configure();
                token.ThrowIfCancellationRequested();

                if (tOuts.Length == 0)
                {
                    if (RequestConfig.ErrorConfig.FailedToFindInGetAllIsError)
                        throw new CrudFailedToFindException { EntityTypeProperty = typeof(TEntity) };

                    var defaultEntity = RequestConfig.GetDefaultFor<TEntity>();
                    if (defaultEntity != null)
                    {
                        tOuts = new TOut[]
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

                tOuts = await resultEntities.CreateResults<TEntity, TOut>(RequestConfig, token).Configure();
            }

            token.ThrowIfCancellationRequested();

            var items = await request.RunResultHooks(RequestConfig, tOuts, token).Configure();

            return items;
        }
    }
}

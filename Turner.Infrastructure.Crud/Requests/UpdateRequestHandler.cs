﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Mediator;

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IUpdateAlgorithm
    {
        DbSet<TEntity> GetEntities<TEntity>(DbContext context)
            where TEntity : class;

        Task SaveChangesAsync(DbContext context);
    }

    public class StandardUpdateAlgorithm : IUpdateAlgorithm
    {
        private readonly IContextAccess _contextAccess;

        public StandardUpdateAlgorithm(IContextAccess contextAccess)
        {
            _contextAccess = contextAccess;
        }

        public DbSet<TEntity> GetEntities<TEntity>(DbContext context)
            where TEntity : class
        {
            return _contextAccess.GetEntities<TEntity>(context);
        }

        public Task SaveChangesAsync(DbContext context)
        {
            return _contextAccess.ApplyChangesAsync(context);
        }
    }

    internal abstract class UpdateRequestHandlerBase<TRequest, TEntity>
        : CrudRequestHandler<TRequest, TEntity>
        where TEntity : class
    {
        protected readonly IUpdateAlgorithm Algorithm;

        public UpdateRequestHandlerBase(DbContext context, 
            CrudConfigManager profileManager,
            IUpdateAlgorithm algorithm)
            : base(context, profileManager)
        {
            Algorithm = algorithm;
        }

        protected async Task<TEntity> GetEntity(TRequest request)
        {
            var selector = RequestConfig.GetSelectorFor<TEntity>(SelectorType.Update);
            var entity = await Algorithm.GetEntities<TEntity>(Context)
                .SelectSingleAsync(request, selector)
                .Configure();
            
            return entity;
        }

        protected async Task UpdateEntity(TRequest request, TEntity entity)
        {
            await RequestConfig.RunPreActionsFor<TEntity>(ActionType.Update, request).Configure();
            await RequestConfig.UpdateEntity(request, entity).Configure();
            await RequestConfig.RunPostActionsFor(ActionType.Update, request, entity).Configure();

            await Algorithm.SaveChangesAsync(Context).Configure();
        }
    }

    internal class UpdateRequestHandler<TRequest, TEntity>
        : UpdateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest>
        where TEntity : class
        where TRequest : IUpdateRequest<TEntity>
    {
        public UpdateRequestHandler(DbContext context, 
            CrudConfigManager profileManager,
            IUpdateAlgorithm algorithm)
            : base(context, profileManager, algorithm)
        {
        }

        public async Task<Response> HandleAsync(TRequest request)
        {
            var entity = default(TEntity);

            try
            {
                entity = await GetEntity(request).Configure();
            }
            catch (CrudRequestFailedException e)
            {
                var error = new RequestFailedError(request, e);
                return ErrorDispatcher.Dispatch(error);
            }

            if (entity == null && RequestConfig.ErrorConfig.FailedToFindInUpdateIsError)
                return ErrorDispatcher.Dispatch(new FailedToFindError(request, typeof(TEntity)));

            if (entity != null)
                await UpdateEntity(request, entity).Configure();

            return Response.Success();
        }
    }

    internal class UpdateRequestHandler<TRequest, TEntity, TOut>
        : UpdateRequestHandlerBase<TRequest, TEntity>,
          IRequestHandler<TRequest, TOut>
        where TEntity : class
        where TRequest : IUpdateRequest<TEntity, TOut>
    {
        public UpdateRequestHandler(DbContext context, 
            CrudConfigManager profileManager,
            IUpdateAlgorithm algorithm)
            : base(context, profileManager, algorithm)
        {
        }

        public async Task<Response<TOut>> HandleAsync(TRequest request)
        {
            var entity = default(TEntity);
            var result = default(TOut);

            try
            {
                entity = await GetEntity(request).Configure();
            }
            catch (CrudRequestFailedException e)
            {
                var error = new RequestFailedError(request, e);
                return ErrorDispatcher.Dispatch<TOut>(error);
            }
            
            if (entity == null && RequestConfig.ErrorConfig.FailedToFindInUpdateIsError)
            {
                var error = new FailedToFindError(request, typeof(TEntity));
                return ErrorDispatcher.Dispatch<TOut>(error);
            }

            if (entity != null)
            {
                await UpdateEntity(request, entity).Configure();
                result = Mapper.Map<TOut>(entity);
            }

            return new Response<TOut> { Data = result };
        }
    }
}

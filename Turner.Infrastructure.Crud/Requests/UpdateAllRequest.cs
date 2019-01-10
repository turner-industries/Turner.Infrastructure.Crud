﻿using System.Collections.Generic;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Decorators;

// ReSharper disable UnusedTypeParameter

namespace Turner.Infrastructure.Crud.Requests
{
    public interface IUpdateAllRequest : IBulkRequest
    {
    }

    public interface IUpdateAllRequest<TEntity> : IUpdateAllRequest, IRequest
        where TEntity : class
    {
    }

    public interface IUpdateAllRequest<TEntity, TOut> : IUpdateAllRequest, IRequest<UpdateAllResult<TOut>>
        where TEntity : class
    {
    }

    public class UpdateAllResult<TOut>
    {
        public List<TOut> Items { get; }

        public UpdateAllResult(List<TOut> items)
        {
            Items = items;
        }
    }

    [DoNotValidate]
    public class UpdateAllRequest<TEntity, TIn> : IUpdateAllRequest<TEntity>
        where TEntity : class
    {
        public UpdateAllRequest(List<TIn> items) { Items = items; }

        public List<TIn> Items { get; }
    }

    [DoNotValidate]
    public class UpdateAllByIdRequest<TEntity, TIn> : UpdateAllRequest<TEntity, TIn>
        where TEntity : class
    {
        public UpdateAllByIdRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByIdRequestProfile<TEntity, TIn>
        : CrudBulkRequestProfile<UpdateAllByIdRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public UpdateAllByIdRequestProfile()
        {
            ForEntity<TEntity>()
                .UseData(request => request.Items)
                .WithKeys("Id");
        }
    }

    [DoNotValidate]
    public class UpdateAllByGuidRequest<TEntity, TIn> : UpdateAllRequest<TEntity, TIn>
        where TEntity : class
    {
        public UpdateAllByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByGuidRequestProfile<TEntity, TIn>
        : CrudBulkRequestProfile<UpdateAllByGuidRequest<TEntity, TIn>, TIn>
        where TEntity : class
    {
        public UpdateAllByGuidRequestProfile()
        {
            ForEntity<TEntity>()
                .UseData(request => request.Items)
                .WithKeys("Guid");
        }
    }

    [DoNotValidate]
    public class UpdateAllRequest<TEntity, TIn, TOut> : IUpdateAllRequest<TEntity, TOut>
        where TEntity : class
    {
        public UpdateAllRequest(List<TIn> items) { Items = items; }

        public List<TIn> Items { get; }
    }
    
    [DoNotValidate]
    public class UpdateAllByIdRequest<TEntity, TIn, TOut> : UpdateAllRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public UpdateAllByIdRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByIdRequestProfile<TEntity, TIn, TOut>
        : CrudBulkRequestProfile<UpdateAllByIdRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public UpdateAllByIdRequestProfile()
        {
            ForEntity<TEntity>()
                .UseData(request => request.Items)
                .WithKeys("Id");
        }
    }

    [DoNotValidate]
    public class UpdateAllByGuidRequest<TEntity, TIn, TOut> : UpdateAllRequest<TEntity, TIn, TOut>
        where TEntity : class
    {
        public UpdateAllByGuidRequest(List<TIn> items) : base(items) { }
    }

    public class UpdateAllByGuidRequestProfile<TEntity, TIn, TOut>
        : CrudBulkRequestProfile<UpdateAllByGuidRequest<TEntity, TIn, TOut>, TIn>
        where TEntity : class
    {
        public UpdateAllByGuidRequestProfile()
        {
            ForEntity<TEntity>()
                .UseData(request => request.Items)
                .WithKeys("Guid");
        }
    }
}

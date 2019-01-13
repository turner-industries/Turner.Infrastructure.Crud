﻿using System.Collections.Generic;
using Turner.Infrastructure.Mediator;
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
}

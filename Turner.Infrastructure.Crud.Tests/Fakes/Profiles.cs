﻿using System;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public class CrudRequestProfile : CrudRequestProfile<ICrudRequest>
    {
        public CrudRequestProfile()
        {
            BeforeCreating(request =>
            {
                if (request is IHasPreMessage tRequest)
                    tRequest.PreMessage = "PreCreate";
            });

            BeforeUpdating(request =>
            {
                if (request is IHasPreMessage tRequest)
                    tRequest.PreMessage = "PreUpdate";
            });

            ForEntity<IEntity>()
                .AfterCreating(entity => entity.PostMessage = "PostCreate/Entity")
                .AfterUpdating(entity => entity.PostMessage = "PostUpdate/Entity");

            ConfigureErrors(config =>
            {
                config.FailedToFindInAnyIsError = true;
            });
        }
    }

    public class DefaultGetRequestProfile<TEntity, TKey, TOut> 
        : CrudRequestProfile<GetRequest<TEntity, TKey, TOut>>
        where TEntity : class, IEntity
    {
        public DefaultGetRequestProfile()
        {
            ForEntity<IEntity>()
                .SelectForGetWith(builder => builder.Build("Key", "Id"));
        }
    }

    public class DefaultUpdateRequestProfile<TEntity, TKey, TIn, TOut>
        : CrudRequestProfile<UpdateRequest<TEntity, TKey, TIn, TOut>>
        where TEntity : class, IEntity
    {
        public DefaultUpdateRequestProfile()
        {
            ForEntity<IEntity>()
                .SelectForUpdateWith(builder => builder.Build(r => r.Key, e => e.Id));
        }
    }
}

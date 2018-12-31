using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public enum SortKeys
    {
        Key1, Key2, Key3, Key4
    }

    public static class SortKeys2
    {
        public const string Key1 = "Key1";
        public const string Key2 = "Key2";
        public const string Key3 = "Key3";
    }

    public class SortEntity : IEntity
    {
        public int Id { get; set; }
        public string PostMessage { get; set; }
        public bool IsDeleted { get; set; }

        public int Col1 { get; set; }
        public string Col2 { get; set; }
        public long Col3 { get; set; }
    }

    public class SortTestRequest : GetAllRequest<SortEntity, UserGetDto>
    {
        public SortKeys SortKey { get; set; }
    }

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

            BeforeDeleting(request =>
            {
                if (request is IHasPreMessage tRequest)
                    tRequest.PreMessage += "PreDelete";
            });

            ConfigureErrors(config =>
            {
                config.FailedToFindInDeleteIsError = true;
                config.FailedToFindInGetAllIsError = true;
                config.FailedToFindInGetIsError = true;
                config.FailedToFindInUpdateIsError = true;
            });

            ForEntity<NonEntity>()
                .ConfigureOptions(config => config.UseProjection = false);

            ForEntity<IEntity>()
                .AfterCreating(entity => entity.PostMessage = "PostCreate/Entity")
                .AfterUpdating(entity => entity.PostMessage = "PostUpdate/Entity")
                .AfterDeleting(entity => entity.PostMessage = "PostDelete/Entity");
        }
    }

    public class DefaultGetRequestProfile<TEntity, TKey, TOut> 
        : CrudRequestProfile<GetRequest<TEntity, TKey, TOut>>
        where TEntity : class
    {
        public DefaultGetRequestProfile()
        {
            ForEntity<IEntity>()
                .SelectWith(builder => builder.Single(r => r.Key, e => e.Id));
        }
    }

    public class DefaultUpdateRequestProfile<TEntity, TKey, TIn, TOut>
        : CrudRequestProfile<UpdateRequest<TEntity, TKey, TIn, TOut>>
        where TEntity : class
    {
        public DefaultUpdateRequestProfile()
        {
            ForEntity<IEntity>()
                .SelectWith(builder => builder.Single(r => r.Key, e => e.Id));
        }
    }

    public class DefaultSaveRequestProfile<TEntity, TKey, TIn, TOut>
        : CrudRequestProfile<SaveRequest<TEntity, TKey, TIn, TOut>>
        where TEntity : class
    {
        public DefaultSaveRequestProfile()
        {
            ForEntity<IEntity>()
                .SelectWith(builder => builder.Single(r => r.Key, e => e.Id));
        }
    }
}

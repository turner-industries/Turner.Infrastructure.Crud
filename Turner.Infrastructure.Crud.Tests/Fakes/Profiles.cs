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
}

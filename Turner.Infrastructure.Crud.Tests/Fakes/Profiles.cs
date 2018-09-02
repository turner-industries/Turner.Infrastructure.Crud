using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public class CrudRequestProfile : CrudRequestProfile<ICrudRequest>
    {
        public CrudRequestProfile()
        {
            ForEntity<IEntity>()
                .AfterCreating(entity =>
                {
                    entity.PostMessage = "PostMessage/Entity";
                    return Task.CompletedTask;
                });

            ConfigureErrors(config =>
            {
                config.FailedToFindIsError = true;
            });
        }
    }
}

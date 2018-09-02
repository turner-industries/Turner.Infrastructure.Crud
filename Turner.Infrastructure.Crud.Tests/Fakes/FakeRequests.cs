using AutoMapper;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    [DoNotValidate]
    public class CreateUserWithResponseRequest : 
        UserDto, ICreateRequest<User, UserGetDto>
    {

    }

    [DoNotValidate]
    public class CreateUserWithoutResponseRequest : 
        ICreateRequest<User>
    {
        public UserDto User { get; set; }
    }

    [DoNotValidate]
    public class DerivedCreateUserWithoutResponseRequest :
        CreateUserWithoutResponseRequest
    {
        public object OtherStuff { get; set; } 
    }

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
        }
    }
    
    public class CreateRequestProfile
        : CrudRequestProfile<ICreateRequest>
    {
        public CreateRequestProfile()
        {
            ForEntity<IHasPreMessage>()
                .BeforeCreating(request =>
                {
                    if (request is IHasPreMessage withMessage)
                        withMessage.PreMessage += "/Entity";

                    return Task.CompletedTask;
                });

            ForEntity<User>()
                .BeforeCreating(request =>
                {
                    if (request is UserDto dto)
                        dto.PreMessage += "/User";

                    return Task.CompletedTask;
                });
        }
    }

    public class CreateUserProfile 
        : CrudRequestProfile<CreateUserWithoutResponseRequest>
    {
        public CreateUserProfile()
        {
            ForEntity<User>()
                .CreateWith(request => Mapper.Map<User>(request.User))
                .AfterCreating(user =>
                {
                    user.PostMessage += "/User";
                    return Task.CompletedTask;
                });
        }
    }

    public class DefaultCreateRequestProfile<TEntity, TIn>
        : CrudRequestProfile<CreateRequest<TEntity, TIn>>
        where TEntity : class
    {
        public DefaultCreateRequestProfile()
        {
            ForEntity<TEntity>()
                .AfterCreating(entity =>
                {
                    if (entity is IEntity ent)
                        ent.PostMessage = "Default";

                    return Task.CompletedTask;
                });
        }
    }
}

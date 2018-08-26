using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    [DoNotValidate]
    public class CreateUserWithResponseRequest : ICreateRequest<User, UserDto, UserGetDto>
    {

    }

    [DoNotValidate]
    public class CreateUserWithoutResponseRequest : ICreateRequest<User, UserDto>
    {

    }
}

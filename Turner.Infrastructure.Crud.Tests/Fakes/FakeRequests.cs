using AutoMapper;
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
        CreateUserWithoutResponseRequest, ICreateRequest<User>
    {
        public object OtherStuff { get; set; } 
    }

    public class CreateUserWithoutResponseProfile 
        : CrudRequestProfile<CreateUserWithoutResponseRequest>
    {
        public CreateUserWithoutResponseProfile()
        {
            ForEntity<User>()
                .CreateWith(request => Mapper.Map<User>(request.User));
        }
    }

    public class FakeRequestsAutoMapperProfiles : Profile
    {
        public FakeRequestsAutoMapperProfiles()
        {
            CreateMap<CreateUserWithResponseRequest, User>()
                .ForMember(x => x.Id, o => o.Ignore());

            CreateMap<UserDto, User>()
                .ForMember(x => x.Id, o => o.Ignore());
        }
    }
}

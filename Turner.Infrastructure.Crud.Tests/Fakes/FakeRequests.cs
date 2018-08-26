using AutoMapper;
using System;
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

    public class CreateUserWithoutResponseProfile : CrudRequestProfile<CreateUserWithoutResponseRequest>
    {   
    }

    public class FakeRequestsAutoMapperProfiles : Profile
    {
        public FakeRequestsAutoMapperProfiles()
        {
            CreateMap<CreateUserWithResponseRequest, User>()
                .ForMember(x => x.Id, o => o.Ignore());

            CreateMap<CreateRequest<User, UserDto>, User>()
                .ForMember(x => x.Id, o => o.Ignore());

            CreateMap<CreateUserWithoutResponseRequest, User>()
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.Name, o => o.MapFrom(x => x.User.Name));

            CreateMap<CreateRequest<User, UserDto, UserGetDto>, User>()
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.Name, o => o.MapFrom(x => x.Data.Name));
        }
    }
}

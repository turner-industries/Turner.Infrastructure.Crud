using AutoMapper;
using System.ComponentModel.DataAnnotations;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public interface IEntity
    {
        int Id { get; set; }
        string PostMessage { get; set; }
    }

    public interface IHasPreMessage
    {
        string PreMessage { get; set; }
    }

    public class Entity : IEntity
    {
        [Key, Required]
        public int Id { get; set; }

        public string PostMessage { get; set; }
    }

    public class User : Entity, IHasPreMessage
    {
        public string Name { get; set; }

        public string PreMessage { get; set; }
    }
    
    public class UserDto : IHasPreMessage
    {
        public string Name { get; set; }

        public string PreMessage { get; set; }
    }
    
    public class UserGetDto : UserDto
    {
        public int Id { get; set; }

        public string PostMessage { get; set; }
    }

    public class UserProfiles : Profile
    {
        public UserProfiles()
        {
            CreateMap<UserDto, User>()
                .ForMember(x => x.Id, o => o.Ignore());
        }
    }

    public class Site : Entity
    {

    }

    public class NonEntity
    {
    }

    public class NonEntityDto
    {
    }
}

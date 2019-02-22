using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public interface IEntity
    {
        int Id { get; set; }

        bool IsDeleted { get; set; }
    }
    
    public class Entity : IEntity
    {
        [Key, Required]
        public int Id { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

    public class User : Entity
    {
        public string Name { get; set; }
    }
    
    public class UserDto
    {
        public string Name { get; set; }
    }
    
    public class UserGetDto : UserDto
    {
        public int Id { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class UserProfiles : Profile
    {
        public UserProfiles()
        {
            CreateMap<UserDto, User>()
                .ForMember(x => x.Id, o => o.Ignore());
        }
    }

    public class UserClaim : Entity
    {
        public int UserId { get; set; }

        public string Claim { get; set; }
    }

    public class Site : Entity
    {
        [Required]
        public Guid Guid { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public class SiteDto
    {
        public Guid Guid { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class SiteGetDto : SiteDto
    {
        public int Id { get; set; }
    }

    public class SiteProfiles : Profile
    {
        public SiteProfiles()
        {
            CreateMap<SiteDto, Site>()
                .ForMember(x => x.Id, o => o.Ignore());
        }
    }

    public class NonEntity
    {
        [Key, Required]
        public int Id { get; set; }
    }

    public interface IHookEntity
    {
        string RequestHookMessage { get; set; }

        string EntityHookMessage { get; set; }

        string ItemHookMessage { get; set; }
    }

    public class HookAutoMapperProfile : Profile
    {
        public HookAutoMapperProfile()
        {
            CreateMap<HookDto, HookEntity>().ReverseMap();
        }
    }

    public class HookEntity : Entity, IHookEntity
    {
        public string RequestHookMessage { get; set; }

        public string EntityHookMessage { get; set; }

        public string ItemHookMessage { get; set; }
    }
}

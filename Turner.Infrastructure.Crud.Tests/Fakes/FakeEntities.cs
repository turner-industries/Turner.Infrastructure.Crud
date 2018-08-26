using System.ComponentModel.DataAnnotations;
using Turner.Infrastructure.Crud.Configuration;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public interface IEntity
    {
        int Id { get; set; }
    }

    public class IEntityCrudProfile : CrudEntityProfile<IEntity>
    {

    }

    public class Entity : IEntity
    {
        [Key, Required]
        public int Id { get; set; }
    }

    public class User : Entity
    {
        public string Name { get; set; }
    }

    public class UserCrudProfile : CrudEntityProfile<User>
    {

    }
    
    public class UserDto
    {
        public string Name { get; set; }
    }

    public class UserDtoCrudProfile : CrudDtoProfile<UserDto>
    {

    }

    public class UserGetDto : UserDto
    {
        public int Id { get; set; }
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

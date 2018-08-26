using Turner.Infrastructure.Crud.Configuration;

namespace Turner.Infrastructure.Crud.Tests.Fakes
{
    public interface IEntity
    {
        int Id { get; set; }
    }

    public class Entity : IEntity
    {
        public int Id { get; set; }
    }

    public class EntityCrudProfile : CrudEntityProfile<Entity>
    {

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

    public class UserGetDtoCrudProfile : CrudDtoProfile<UserGetDto>
    {

    }
}

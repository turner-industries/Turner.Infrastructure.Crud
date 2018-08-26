namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudDtoProfile
    {

    }

    public class CrudDtoProfile<T> : ICrudDtoProfile
        where T : class
    {

    }
}

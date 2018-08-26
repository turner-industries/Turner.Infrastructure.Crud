namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudDtoProfile
    {

    }

    public abstract class CrudDtoProfile<T> : ICrudDtoProfile
    {
    }

    public class DefaultCrudDtoProfile<T> : CrudDtoProfile<T>
    {
    }
}

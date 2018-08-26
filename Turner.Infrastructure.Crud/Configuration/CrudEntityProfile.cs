namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudEntityProfile
    {
    }

    public abstract class CrudEntityProfile<T> : ICrudEntityProfile
    {
    }

    public class DefaultCrudEntityProfile<T> : CrudEntityProfile<T>
    {
    }
}

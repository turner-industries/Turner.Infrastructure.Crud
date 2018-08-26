namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudEntityProfile
    {

    }

    public class CrudEntityProfile<T> : ICrudEntityProfile
        where T : class
    {

    }
}

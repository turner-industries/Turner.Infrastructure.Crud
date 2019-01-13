// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public interface ICrudRequestEntityConfigBuilder
    {
        void Build<TRequest>(CrudRequestConfig<TRequest> config);
    }
}

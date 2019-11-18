// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud.Configuration.Builders
{
    public interface IRequestEntityConfigBuilder
    {
        void Build<TRequest>(RequestConfig<TRequest> config);
    }
}

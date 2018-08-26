using AutoMapper;
using System;

namespace Turner.Infrastructure.Crud.Configuration
{
    public interface ICrudRequestProfile
    {
        Func<TRequest, TEntity> ConvertToEntity<TRequest, TEntity>();
    }
    
    public abstract class CrudRequestProfile<TRequest> 
        : ICrudRequestProfile
    {
        public virtual Func<TCompatibleRequest, TEntity> ConvertToEntity<TCompatibleRequest, TEntity>()
        {
            if (!typeof(TRequest).IsAssignableFrom(typeof(TCompatibleRequest)))
                throw new BadCrudConfigurationException();

            return tRequest => Mapper.Map<TEntity>(tRequest);
        }
    }

    public class DefaultCrudRequestProfile<TRequest>
        : CrudRequestProfile<TRequest>
    {
    }
}

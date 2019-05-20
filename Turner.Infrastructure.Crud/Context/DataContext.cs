using Turner.Infrastructure.Crud.Configuration;

namespace Turner.Infrastructure.Crud.Context
{
    public class DataContext<TEntity>
        where TEntity : class
    {
        public EntitySet<TEntity> EntitySet { get; private set; }

        public ICrudRequestConfig Configuration { get; }

        public DataContext(ICrudRequestConfig config) : this(null, config) { }

        public DataContext(EntitySet<TEntity> entitySet, ICrudRequestConfig config)
        {
            EntitySet = entitySet;
            Configuration = config;
        }

        internal DataContext<TEntity> WithEntitySet(EntitySet<TEntity> entitySet)
        {
            EntitySet = entitySet;

            return this;
        }
    }
}

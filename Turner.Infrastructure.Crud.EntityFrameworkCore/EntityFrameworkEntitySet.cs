using Microsoft.EntityFrameworkCore;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.EntityFrameworkCore
{
    public class EntityFrameworkEntitySet<TEntity> : EntitySet<TEntity>
        where TEntity : class
    {
        public DbSet<TEntity> Set { get; }

        public EntityFrameworkEntitySet(DbSet<TEntity> set, 
            IDataAgent dataAgent)
            : base(EntityFrameworkEntityQueryable<TEntity>.From(set), dataAgent)
        {
            Set = set;
        }
    }
}

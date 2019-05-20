using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Turner.Infrastructure.Crud.Context;

namespace Turner.Infrastructure.Crud.Tests.ContextTests
{
    public interface IInMemorySet
    {
    }

    public class InMemorySet<TEntity> : EntitySet<TEntity>, IInMemorySet
        where TEntity : class
    {
        public List<TEntity> Items { get; }

        public int Id { get; set; } = 1;
        
        public InMemorySet(List<TEntity> items, IDataAgentFactory dataAgentFactory)
            : base(CreateEntityQueryable(items), dataAgentFactory)
        {
            Items = items;
        }

        private static EntityQueryable<TEntity> CreateEntityQueryable(List<TEntity> items)
        {
            var data = items.AsQueryable();

            return new EntityQueryable<TEntity>(data.Provider, Expression.Constant(data));
        }
    }
}

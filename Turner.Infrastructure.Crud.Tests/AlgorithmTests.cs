using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Algorithms;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Configuration;

namespace Turner.Infrastructure.Crud.Tests
{
    [TestFixture]
    public class AlgorithmTests
    {
        private Scope _scope;

        protected Container Container;

        protected IMediator Mediator { get; private set; }

        protected IEntityContext Context { get; private set; }

        [TearDown]
        public void TearDown()
        {
            _scope.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            var assemblies = new[] { typeof(UnitTestSetUp).Assembly };
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            UnitTestSetUp.ConfigureDatabase(container);

            container.ConfigureMediator(assemblies);
            container.ConfigureCrud(assemblies);

            container.Options.AllowOverridingRegistrations = true;
            container.Register<IEntityContext, InMemoryContext>();
            container.Options.AllowOverridingRegistrations = false;

            _scope = AsyncScopedLifestyle.BeginScope(container);

            Mediator = _scope.GetInstance<IMediator>();
            Context = _scope.GetInstance<IEntityContext>();

            Container = container;
        }

        [Test]
        public async Task Handle_CustomContext_DoesNotHaveErrors()
        {
            var request = new CreateRequest<User, UserDto, UserGetDto>(new UserDto
            {
                Name = "Test"
            });

            await Mediator.HandleAsync(request);
            
            Assert.IsNotNull(Context.EntitySet<User>());

            var users = await Context.CountAsync(Context.EntitySet<User>());
            Assert.AreEqual(1, users);

            var user = await Mediator.HandleAsync(new GetByIdRequest<User, UserGetDto>(1));
            Assert.IsNotNull(user);
            Assert.IsNotNull(user.Data);
            Assert.AreEqual(request.Data.Name, user.Data.Name);
        }
    }
    
    public interface IInMemorySet { }

    public class InMemorySet<TEntity> : IEntitySet<TEntity>, IInMemorySet, IAsyncEnumerable<TEntity>
    {
        private List<TEntity> _items = new List<TEntity>();
        private int _id = 1;

        public int Count => _items.Count;

        public Type ElementType => _items.AsQueryable().ElementType;

        public Expression Expression => _items.AsQueryable().Expression;

        public IQueryProvider Provider => _items.AsQueryable().Provider;

        public TEntity Create(TEntity entity)
        {
            if (entity is IEntity entityWithId)
                entityWithId.Id = _id++;

            _items.Add(entity);

            return entity;
        }

        public TEntity[] Create(IEnumerable<TEntity> entities) =>
            entities.Select(Create).ToArray();

        public Task<TEntity> CreateAsync(TEntity entity, CancellationToken token = default(CancellationToken)) => 
            Task.FromResult(Create(entity));

        public Task<TEntity[]> CreateAsync(IEnumerable<TEntity> entities, CancellationToken token = default(CancellationToken)) =>
            Task.FromResult(Create(entities));

        public TEntity Update(TEntity entity)
            => entity;

        public TEntity[] Update(IEnumerable<TEntity> entities)
            => entities.ToArray();

        public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken token = default(CancellationToken))
            => Task.FromResult(Update(entity));

        public Task<TEntity[]> UpdateAsync(IEnumerable<TEntity> entities, CancellationToken token = default(CancellationToken))
            => Task.FromResult(Update(entities));
        
        public TEntity Delete(TEntity entity)
        {
            _items.Remove(entity);
            return entity;
        }

        public TEntity[] Delete(IEnumerable<TEntity> entities) => 
            entities.Select(Delete).ToArray();

        public Task<TEntity> DeleteAsync(TEntity entity, CancellationToken token = default(CancellationToken))
            => Task.FromResult(Delete(entity));

        public Task<TEntity[]> DeleteAsync(IEnumerable<TEntity> entities, CancellationToken token = default(CancellationToken))
            => Task.FromResult(Delete(entities));

        public async Task DeleteAsync(IQueryable<TEntity> entities, CancellationToken token = default(CancellationToken))
        {
            await DeleteAsync(await entities.ToArrayAsync(), token);
        }

        public IEnumerator<TEntity> GetEnumerator()
            => _items.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
            => _items.GetEnumerator();

        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetEnumerator()
            => _items.ToAsyncEnumerable().GetEnumerator();
    }

    public class InMemoryContext : IEntityContext
    {
        private static readonly Dictionary<Type, IInMemorySet> _sets
            = new Dictionary<Type, IInMemorySet>();

        public int ApplyChanges() => 0;

        public Task<int> ApplyChangesAsync(CancellationToken token = default(CancellationToken))
            => Task.FromResult(0);

        public Task<int> CountAsync<T>(IQueryable<T> entities, 
            CancellationToken token = default(CancellationToken))
        {
            var tEntity = typeof(T);
            if (!_sets.TryGetValue(tEntity, out var genericSet))
                return Task.FromResult(0);

            var set = genericSet as InMemorySet<T>;
            return Task.FromResult(set.Count);
        }

        public IEntitySet<TEntity> EntitySet<TEntity>() 
            where TEntity : class
        {
            if (!_sets.TryGetValue(typeof(TEntity), out var set))
                _sets[typeof(TEntity)] = new InMemorySet<TEntity>();

            return _sets[typeof(TEntity)] as InMemorySet<TEntity>;
        }

        public Task<T> SingleOrDefaultAsync<T>(IQueryable<T> entities, 
            CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(entities.SingleOrDefault());
        }

        public Task<T> SingleOrDefaultAsync<T>(IQueryable<T> entities, 
            Expression<Func<T, bool>> predicate, 
            CancellationToken token = default(CancellationToken))
        {
            return Task.FromResult(entities.SingleOrDefault(predicate));
        }

        public Task<T[]> ToArrayAsync<T>(IQueryable<T> entities, 
            CancellationToken token = default(CancellationToken))
        {
            return entities.ToArrayAsync();
        }

        public Task<List<T>> ToListAsync<T>(IQueryable<T> entities, 
            CancellationToken token = default(CancellationToken))
        {
            return entities.ToListAsync();
        }
    }
}

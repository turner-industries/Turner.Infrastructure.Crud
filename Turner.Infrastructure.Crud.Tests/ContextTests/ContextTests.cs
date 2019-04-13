using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Turner.Infrastructure.Crud.Context;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Configuration;

namespace Turner.Infrastructure.Crud.Tests.ContextTests
{
    [TestFixture]
    public class ContextTests
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

            Crud.CreateInitializer(container, assemblies)
                .Initialize();

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
            
            Assert.IsNotNull(Context.Set<User>());

            var users = await Context.Set<User>().CountAsync();
            Assert.AreEqual(1, users);

            var user = await Mediator.HandleAsync(new GetByIdRequest<User, UserGetDto>(1));
            Assert.IsNotNull(user);
            Assert.IsNotNull(user.Data);
            Assert.AreEqual(request.Item.Name, user.Data.Name);
        }
    }
}

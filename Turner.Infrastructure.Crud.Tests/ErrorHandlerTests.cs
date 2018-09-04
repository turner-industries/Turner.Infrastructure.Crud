using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator;
using Turner.Infrastructure.Mediator.Configuration;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests
{
    [TestFixture]
    public class DefaultErrorHandlerTests
    {
        private Scope _scope;

        protected Container Container;

        protected IMediator Mediator { get; private set; }

        protected DbContext Context { get; private set; }

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
            container.Register<ICrudErrorHandler, TestErrorHandler>(Lifestyle.Singleton);
            container.Options.AllowOverridingRegistrations = false;

            _scope = AsyncScopedLifestyle.BeginScope(container);

            Mediator = _scope.GetInstance<IMediator>();
            Context = _scope.GetInstance<DbContext>();
            Context.Database.EnsureDeleted();
        
            Container = container;
        }

        [Test]
        public async Task Handle_FailingRequest_UsesTestErrorHandler()
        {
            var request = new GetByIdRequest<NonEntity, NonEntity>(1);
            var response = await Mediator.HandleAsync(request);

            Assert.IsTrue(response.HasErrors);
            Assert.AreEqual("Failed to find entity.", response.Errors[0].ErrorMessage);
        }
    }

    [TestFixture]
    public class CustomErrorHandlerTests : BaseUnitTest
    {
        [Test]
        public async Task Handle_UseDefaultErrorHandler_UsesDefaultErrorHandler()
        {
            FailedToFindException exception = null;
            
            try
            {
                await Mediator.HandleAsync(new UseDefaultErrorHandler { Id = 1 });
            }
            catch(FailedToFindException e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
        }

        [Test]
        public async Task Handle_UseCustomErrorHandlerForRequest_UsesCustomErrorHandler()
        {
            var request = new UseCustomErrorHandlerForRequest { Id = 1 };
            var response = await Mediator.HandleAsync(request);

            Assert.IsTrue(response.HasErrors);
            Assert.AreEqual("Failed to find entity.", response.Errors[0].ErrorMessage);
        }

        [Test]
        public async Task Handle_UseCustomErrorHandlerForEntity_UsesCustomErrorHandler()
        {
            var request = new UseCustomErrorHandlerForEntity { Id = 1 };
            var response = await Mediator.HandleAsync(request);

            Assert.IsTrue(response.HasErrors);
            Assert.AreEqual("Failed to find entity.", response.Errors[0].ErrorMessage);
        }
    }

    [DoNotValidate]
    public class UseDefaultErrorHandler
        : IGetRequest<NonEntity, NonEntity>
    {
        public int Id { get; set; }
    }

    public class UseDefaultErrorHandlerProfile
        : CrudRequestProfile<UseDefaultErrorHandler>
    {
        public UseDefaultErrorHandlerProfile()
        {
            ForEntity<NonEntity>()
                .SelectForAnyWith(b => b.Build("Id"));
        }
    }

    [DoNotValidate]
    public class UseCustomErrorHandlerForRequest
        : IGetRequest<NonEntity, NonEntity>
    {
        public int Id { get; set; }
    }

    public class UseCustomErrorHandlerForRequestProfile
        : CrudRequestProfile<UseCustomErrorHandlerForRequest>
    {
        public UseCustomErrorHandlerForRequestProfile()
        {
            ConfigureErrors(config => 
                config.ErrorHandlerFactory = () => new TestErrorHandler());

            ForEntity<NonEntity>()
                .SelectForAnyWith(b => b.Build("Id"));
        }
    }

    [DoNotValidate]
    public class UseCustomErrorHandlerForEntity
        : IGetRequest<NonEntity, NonEntity>
    { 
        public int Id { get; set; }
    }

    public class UseCustomErrorHandlerForEntityProfile
        : CrudRequestProfile<UseCustomErrorHandlerForEntity>
    {
        public UseCustomErrorHandlerForEntityProfile()
        {
            ForEntity<NonEntity>()
                .SelectForAnyWith(b => b.Build("Id"))
                .UseErrorHandlerFactory(() => new TestErrorHandler());
        }
    }
}

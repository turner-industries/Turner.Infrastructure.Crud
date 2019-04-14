using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.EntityFrameworkCore;
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

            Crud.CreateInitializer(container, assemblies)
                .UseEntityFramework()
                .Initialize();
            
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
            Exception exception = null;

            try
            {
                var request = new GetByIdRequest<NonEntity, NonEntity>(1);
                await Mediator.HandleAsync(request);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual("Failed to find entity.", exception.Message);
        }
    }

    [TestFixture]
    public class CustomErrorHandlerTests : BaseUnitTest
    {
        [Test]
        public async Task Handle_UseDefaultErrorHandler_UsesDefaultErrorHandler()
        {
            var request = new UseDefaultErrorHandler { Id = 1 };
            var response = await Mediator.HandleAsync(request);
            
            Assert.IsTrue(response.HasErrors);
            Assert.AreEqual("Failed to find entity.", response.Errors[0].ErrorMessage);
        }

        [Test]
        public async Task Handle_UseCustomErrorHandlerForRequest_UsesCustomErrorHandler()
        {
            Exception exception = null;

            try
            {
                var request = new UseCustomErrorHandlerForRequest { Id = 1 };
                await Mediator.HandleAsync(request);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual("Failed to find entity.", exception.Message);
        }

        [Test]
        public async Task Handle_UseCustomErrorHandlerForEntity_UsesCustomErrorHandler()
        {
            Exception exception = null;

            try
            {
                var request = new UseCustomErrorHandlerForEntity { Id = 1 };
                await Mediator.HandleAsync(request);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual("Failed to find entity.", exception.Message);
        }
    }

    [TestFixture]
    public class TestTypeErrorHandlerTests : BaseUnitTest
    {
        public static ErrorTypeTestHandler TypeTestHandler { get; }
            = new ErrorTypeTestHandler();

        [Test]
        public async Task Handle_WithHookFailure_DispatchesHookFailedError()
        {
            TypeTestHandler.Clear();

            var request = new HookFailureTestRequest();
            await Mediator.HandleAsync(request);

            Assert.AreEqual(typeof(HookFailedError), TypeTestHandler.LastErrorType);
            Assert.AreEqual("HookTest", TypeTestHandler.LastError);
        }

        [Test]
        public async Task Handle_WithCancelation_DispatchesRequestCanceledError()
        {
            TypeTestHandler.Clear();

            var request = new RequestCanceledTestRequest();
            await Mediator.HandleAsync(request);

            Assert.AreEqual(typeof(RequestCanceledError), TypeTestHandler.LastErrorType);
            Assert.AreEqual("CancelTest", TypeTestHandler.LastError);
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
                .SelectWith(b => b.Single("Id"));
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
                .SelectWith(b => b.Single("Id"));
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
                .SelectWith(b => b.Single("Id"))
                .UseErrorHandlerFactory(() => new TestErrorHandler());
        }
    }

    [DoNotValidate]
    public class HookFailureTestRequest
        : IGetRequest<NonEntity, NonEntity>
    {
        public int Id { get; set; }
    }

    public class HookFailureTestProfile
        : CrudRequestProfile<HookFailureTestRequest>
    {
        public HookFailureTestProfile()
        {
            ForEntity<NonEntity>()
                .UseKeys("Id")
                .UseErrorHandlerFactory(() => TestTypeErrorHandlerTests.TypeTestHandler)
                .AddEntityHook((r, e) => throw new InvalidOperationException("HookTest"));
        }
    }

    [DoNotValidate]
    public class RequestCanceledTestRequest
        : IGetRequest<NonEntity, NonEntity>
    {
        public int Id { get; set; }
    }

    public class RequestCanceledTestProfile
        : CrudRequestProfile<RequestCanceledTestRequest>
    {
        public RequestCanceledTestProfile()
        {
            AddRequestHook(r => throw new OperationCanceledException("CancelTest"));

            ForEntity<NonEntity>()
                .UseKeys("Id")
                .UseErrorHandlerFactory(() => TestTypeErrorHandlerTests.TypeTestHandler);
        }
    }

    public class ErrorTypeTestHandler : CrudErrorHandler
    {
        public Type LastErrorType { get; private set; }

        public string LastError { get; private set; }

        public void Clear()
        {
            LastErrorType = null;
            LastError = null;
        }

        private Response Handle(CrudError error)
        {
            LastErrorType = error.GetType();
            LastError = error.Exception?.Message;
            
            return Response.Success();
        }

        protected override Response HandleError(FailedToFindError error)
            => Handle(error);

        protected override Response HandleError(CrudError error)
            => Handle(error);

        protected override Response HandleError(HookFailedError error)
            => Handle(error);

        protected override Response HandleError(RequestCanceledError error)
            => Handle(error);

        protected override Response HandleError(RequestFailedError error)
            => Handle(error);
    }
}

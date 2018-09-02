using AutoMapper;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests
{
    [TestFixture]
    public class CreateRequestTests : BaseUnitTest
    {
        [Test]
        public async Task Handle_WithoutResponse_CreatesUser()
        {
            var request = new CreateUserWithoutResponseRequest
            {
                User = new UserDto { Name = "TestUser" }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());

            var user = Context.Set<User>().FirstOrDefault();
            Assert.IsNotNull(user);
            Assert.AreEqual("TestUser", user.Name);
            Assert.AreEqual("PreMessage", user.PreMessage);
            Assert.AreEqual("PostMessage/Entity/User", user.PostMessage);
        }

        [Test]
        public async Task Handle_DerivedWithoutResponse_CreatesUserUsingBaseConfig()
        {
            var request = new DerivedCreateUserWithoutResponseRequest
            {
                User = new UserDto { Name = "TestUser" },
                OtherStuff = new object()
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());

            var user = Context.Set<User>().FirstOrDefault();
            Assert.IsNotNull(user);
            Assert.AreEqual("TestUser", user.Name);
            Assert.AreEqual("PreMessage", user.PreMessage);
            Assert.AreEqual("PostMessage/Entity/User", user.PostMessage);
        }

        [Test]
        public async Task Handle_WithResponse_CreatesUserAndReturnsDto()
        {
            var request = new CreateUserWithResponseRequest
            {
                Name = "TestUser"
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
            Assert.IsNotNull(response.Data);
            Assert.AreEqual("TestUser", response.Data.Name);
            Assert.AreEqual(response.Data.Id, Context.Set<User>().First().Id);
            Assert.AreEqual("PreMessage/Entity/User", response.Data.PreMessage);
            Assert.AreEqual("PostMessage/Entity", response.Data.PostMessage);
        }

        [Test]
        public async Task Handle_DefaultWithoutResponse_CreatesUser()
        {
            var request = new CreateRequest<User, UserDto>(new UserDto
            {
                Name = "TestUser"
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());

            var user = Context.Set<User>().FirstOrDefault();
            Assert.IsNotNull(user);
            Assert.AreEqual("TestUser", user.Name);
            Assert.AreEqual("PreMessage", user.PreMessage);
            Assert.AreEqual("Default", user.PostMessage);
        }

        [Test]
        public async Task Handle_DefaultWithResponse_CreatesUserAndReturnsDto()
        {
            var request = new CreateRequest<User, UserDto, UserGetDto>(new UserDto { Name = "TestUser" });
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
            Assert.IsNotNull(response.Data);
            Assert.AreEqual("TestUser", response.Data.Name);
            Assert.AreEqual(response.Data.Id, Context.Set<User>().First().Id);
            Assert.AreEqual("PreMessage", response.Data.PreMessage);
            Assert.AreEqual("PostMessage/Entity", response.Data.PostMessage);
        }
    }

    [DoNotValidate]
    public class CreateUserWithResponseRequest : UserDto, ICreateRequest<User, UserGetDto>
    { }

    [DoNotValidate]
    public class CreateUserWithoutResponseRequest : ICreateRequest<User>
    {
        public UserDto User { get; set; }
    }

    [DoNotValidate]
    public class DerivedCreateUserWithoutResponseRequest : CreateUserWithoutResponseRequest
    {
        public object OtherStuff { get; set; }
    }
    
    public class CreateRequestProfile : CrudRequestProfile<ICreateRequest>
    {
        public CreateRequestProfile()
        {
            ForEntity<IHasPreMessage>()
                .BeforeCreating(request =>
                {
                    if (request is IHasPreMessage withMessage)
                        withMessage.PreMessage += "/Entity";

                    return Task.CompletedTask;
                });

            ForEntity<User>()
                .BeforeCreating(request =>
                {
                    if (request is UserDto dto)
                        dto.PreMessage += "/User";

                    return Task.CompletedTask;
                });
        }
    }

    public class CreateUserWithoutResponseProfile : CrudRequestProfile<CreateUserWithoutResponseRequest>
    {
        public CreateUserWithoutResponseProfile()
        {
            ForEntity<User>()
                .CreateWith(request => Mapper.Map<User>(request.User))
                .AfterCreating(user =>
                {
                    user.PostMessage += "/User";
                    return Task.CompletedTask;
                });
        }
    }

    public class DefaultCreateRequestProfile<TEntity, TIn> 
        : CrudRequestProfile<CreateRequest<TEntity, TIn>>
        where TEntity : class
    {
        public DefaultCreateRequestProfile()
        {
            ForEntity<TEntity>()
                .AfterCreating(entity =>
                {
                    if (entity is IEntity ent)
                        ent.PostMessage = "Default";

                    return Task.CompletedTask;
                });
        }
    }
}
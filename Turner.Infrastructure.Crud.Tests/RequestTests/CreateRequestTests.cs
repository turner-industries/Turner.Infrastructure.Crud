using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;

namespace Turner.Infrastructure.Crud.Tests.RequestTests
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
            Assert.AreEqual("TestUser_Modified", response.Data.Name);
            Assert.AreEqual(response.Data.Id, Context.Set<User>().First().Id);
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
        }
    }
    
    public class CreateUserWithResponseRequest : UserDto, ICreateRequest<User, UserGetDto>
    { }
    
    public class CreateUserWithoutResponseRequest : ICreateRequest<User>
    {
        public UserDto User { get; set; }
    }
    
    public class DerivedCreateUserWithoutResponseRequest : CreateUserWithoutResponseRequest
    {
        public object OtherStuff { get; set; }
    }

    public class CreateUserWithResponseProfile
        : RequestProfile<CreateUserWithResponseRequest>
    {
        public CreateUserWithResponseProfile()
        {
            ForEntity<User>()
                .CreateResultWith(user =>
                {
                    var result = Mapper.Map<UserGetDto>(user);
                    result.Name += "_Modified";
                    return result;
                });
        }
    }

    public class CreateUserWithoutResponseProfile 
        : RequestProfile<CreateUserWithoutResponseRequest>
    {
        public CreateUserWithoutResponseProfile()
        {
            ForEntity<User>()
                .CreateEntityWith(request => Mapper.Map<User>(request.User));
        }
    }
}
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;

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
        }

        [Test]
        public async Task Handle_DerivedWithoutResponse_CreatesUserUsingBaseConfig()
        {
            var request = new DerivedCreateUserWithoutResponseRequest
            {
                User = new UserDto { Name = "TestUser" }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
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
        }

        [Test]
        public async Task Handle_DefaultWithoutResponse_CreatesUser()
        {
            var request = new CreateRequest<User, UserDto>(new UserDto { Name = "TestUser" });
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(1, Context.Set<User>().Count());
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
}
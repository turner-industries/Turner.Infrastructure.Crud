using NUnit.Framework;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests.RequestTests
{
    [TestFixture]
    public class GetAllRequestTests : BaseUnitTest
    {
        [Test]
        public async Task Handle_GetUsersWithDefaultWithError_ReturnsDefaultAndError()
        {
            var request = new GetUsersWithDefaultWithErrorRequest();
            var response = await Mediator.HandleAsync(request);

            Assert.IsTrue(response.HasErrors);
            Assert.AreEqual("Failed to find entity.", response.Errors[0].ErrorMessage);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(1, response.Data.Count);
            Assert.AreEqual("DefaultUser", response.Data[0].Name);
        }

        [Test]
        public async Task Handle_GetUsersWithDefaultWithoutError_ReturnsDefault()
        {
            var request = new GetUsersWithDefaultWithoutErrorRequest();
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(1, response.Data.Count);
            Assert.AreEqual("DefaultUser", response.Data[0].Name);
        }

        [Test]
        public async Task Handle_GetUsersWithoutDefaultWithError_ReturnsError()
        {
            var request = new GetUsersWithoutDefaultWithErrorRequest();
            var response = await Mediator.HandleAsync(request);

            Assert.IsTrue(response.HasErrors);
            Assert.AreEqual("Failed to find entity.", response.Errors[0].ErrorMessage);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(0, response.Data.Count);
        }

        [Test]
        public async Task Handle_GetUsersWithoutDefaultWithoutError_ReturnsEmptyList()
        {
            var request = new GetUsersWithoutDefaultWithoutErrorRequest();
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(0, response.Data.Count);
        }

        [Test]
        public async Task Handle_DefaultGetAllRequest_ReturnsAllEntities()
        {
            await Context.AddRangeAsync(new User { Name = "User1" }, new User { Name = "User2" });
            await Context.SaveChangesAsync();

            var request = new GetAllRequest<User, UserGetDto>();
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(2, response.Data.Count);
        }

        [Test]
        public async Task Handle_UnprojectedGetAllRequest_ReturnsAllEntities()
        {
            await Context.AddRangeAsync(new User { Name = "User1" }, new User { Name = "User2" });
            await Context.SaveChangesAsync();

            var request = new GetUsersUnprojectedRequest();
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(2, response.Data.Count);
        }
    }

    [DoNotValidate]
    public class GetUsersWithDefaultWithErrorRequest 
        : IGetAllRequest<User, UserGetDto>
    { }

    [DoNotValidate]
    public class GetUsersWithDefaultWithoutErrorRequest 
        : IGetAllRequest<User, UserGetDto>
    { }

    [DoNotValidate]
    public class GetUsersWithoutDefaultWithErrorRequest 
        : IGetAllRequest<User, UserGetDto>
    { }

    [DoNotValidate]
    public class GetUsersWithoutDefaultWithoutErrorRequest 
        : IGetAllRequest<User, UserGetDto>
    { }

    public class GetWithDefaultWithErrorProfile : CrudRequestProfile<GetUsersWithDefaultWithErrorRequest>
    {
        public GetWithDefaultWithErrorProfile()
        {
            ConfigureErrors(config => config.FailedToFindInGetAllIsError = true);
            ForEntity<User>().UseDefault(new User { Name = "DefaultUser" });
        }
    }

    public class GetWithDefaultWithoutErrorProfile : CrudRequestProfile<GetUsersWithDefaultWithoutErrorRequest>
    {
        public GetWithDefaultWithoutErrorProfile()
        {
            ForEntity<User>().UseDefault(new User { Name = "DefaultUser" });
            ConfigureErrors(config => config.FailedToFindInGetAllIsError = false);
        }
    }

    public class GetWithoutDefaultWithErrorProfile : CrudRequestProfile<GetUsersWithoutDefaultWithErrorRequest>
    {
        public GetWithoutDefaultWithErrorProfile()
        {
            ConfigureErrors(config => config.FailedToFindInGetAllIsError = true);
        }
    }

    public class GetWithoutDefaultWithoutErrorProfile : CrudRequestProfile<GetUsersWithoutDefaultWithoutErrorRequest>
    {
        public GetWithoutDefaultWithoutErrorProfile()
        {
            ConfigureErrors(config => config.FailedToFindInGetAllIsError = false);
        }
    }

    [DoNotValidate]
    public class GetUsersUnprojectedRequest : GetAllRequest<User, UserGetDto> { }

    public class GetUsersUnprojectedProfile : CrudRequestProfile<GetUsersUnprojectedRequest>
    {
        public GetUsersUnprojectedProfile()
        {
            ConfigureOptions(config => config.UseProjection = false);
        }
    }
}
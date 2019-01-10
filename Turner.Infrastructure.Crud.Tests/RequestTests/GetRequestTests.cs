using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests.RequestTests
{
    [TestFixture]
    public class GetRequestTests : BaseUnitTest
    {
        User _user;
        Site _site;

        [SetUp]
        public void SetUp()
        {
            _user = new User { Name = "TestUser" };
            _site = new Site { Guid = Guid.NewGuid() };

            Context.Add(_user);
            Context.Add(_site);

            Context.SaveChanges();
        }

        [Test]
        public async Task Handle_GenericGetByIdRequest_GetsUser()
        {
            var request = new GetByIdRequest<User, UserGetDto>(_user.Id);
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_user.Id, response.Data.Id);
            Assert.AreEqual(_user.Name, response.Data.Name);
            Assert.AreEqual(_user.PreMessage, response.Data.PreMessage);
            Assert.AreEqual(_user.PostMessage, response.Data.PostMessage);
        }

        [Test]
        public async Task Handle_GenericGetByGuidRequest_GetsSite()
        {
            var request = new GetByGuidRequest<Site, SiteGetDto>(_site.Guid);
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_site.Id, response.Data.Id);
            Assert.AreEqual(_site.Guid, response.Data.Guid);
            Assert.AreEqual(_site.PreMessage, response.Data.PreMessage);
            Assert.AreEqual(_site.PostMessage, response.Data.PostMessage);
        }

        [Test]
        public async Task Handle_GetUserByIdRequest_GetsUser()
        {
            var request = new GetUserByIdRequest { Id = _user.Id };
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_user.Id, response.Data.Id);
            Assert.AreEqual(_user.Name, response.Data.Name);
            Assert.AreEqual(_user.PreMessage, response.Data.PreMessage);
            Assert.AreEqual(_user.PostMessage, response.Data.PostMessage);
        }

        [Test]
        public async Task Handle_GetUserByNameRequest_GetsUser()
        {
            var request = new GetUserByNameRequest { Name = _user.Name.ToUpper() };
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_user.Id, response.Data.Id);
            Assert.AreEqual(_user.Name, response.Data.Name);
            Assert.AreEqual(_user.PreMessage, response.Data.PreMessage);
            Assert.AreEqual(_user.PostMessage, response.Data.PostMessage);
        }

        [Test]
        public async Task Handle_InvalidGetUserByIdRequest_ReturnsError()
        {
            var request = new GetUserByIdRequest { Id = 100 };
            var response = await Mediator.HandleAsync(request);
            
            Assert.IsTrue(response.HasErrors);
        }

        [Test]
        public async Task Handle_InvalidGetUserByNameRequest_ReturnsDefault()
        {
            var request = new GetUserByNameRequest { Name = "NoSuchUser" };
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual("DefaultUser", response.Data.Name);
        }

        [Test]
        public async Task Handle_DefaultGetRequest_GetsUser()
        {
            var request = new GetRequest<User, int, UserGetDto>(_user.Id);
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_user.Id, response.Data.Id);
            Assert.AreEqual(_user.Name, response.Data.Name);
            Assert.AreEqual(_user.PreMessage, response.Data.PreMessage);
            Assert.AreEqual(_user.PostMessage, response.Data.PostMessage);
        }

        [Test]
        public async Task Handle_GetUserByPrimaryKeyRequest_GetsUser()
        {
            var request = new GetUserByKeyRequest { Id = _user.Id, Name = _user.Name };
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_user.Id, response.Data.Id);
            Assert.AreEqual(_user.Name, response.Data.Name);
            Assert.AreEqual(_user.PreMessage, response.Data.PreMessage);
            Assert.AreEqual(_user.PostMessage, response.Data.PostMessage);
        }
    }

    [DoNotValidate]
    public class GetUserByIdRequest : IGetRequest<User, UserGetDto>
    {
        public int Id { get; set; }
    }
    
    [DoNotValidate]
    public class GetUserByNameRequest : IGetRequest<User, UserGetDto>
    {
        public string Name { get; set; }
    }

    [DoNotValidate]
    public class GetUserByKeyRequest : IGetRequest<User, UserGetDto>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class GetUserByIdProfile 
        : CrudRequestProfile<GetUserByIdRequest>
    {
        public GetUserByIdProfile()
        {
            ForEntity<User>()
                .SelectWith(builder => builder.Single("Id"))
                .UseDefault(new User { Name = "DefaultUser" });
        }
    }

    public class GetUserByNameProfile 
        : CrudRequestProfile<GetUserByNameRequest>
    {
        public GetUserByNameProfile()
        {
            ForEntity<User>()
                .UseDefault(new User { Name = "DefaultUser" })
                .SelectWith(builder =>
                    builder.Single((request, entity) =>
                        string.Equals(entity.Name, request.Name, StringComparison.InvariantCultureIgnoreCase)));

            ConfigureErrors(config => config.FailedToFindInGetIsError = false);
        }
    }

    public class GetUserByKeyProfile 
        : CrudRequestProfile<GetUserByKeyRequest>
    {
        public GetUserByKeyProfile()
        {
            ForEntity<User>().WithKeys("Id");
        }
    }
}
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests
{
    [TestFixture]
    public class GetRequestTests : BaseUnitTest
    {
        User _user;

        [SetUp]
        public void SetUp()
        {
            _user = new User { Name = "TestUser" };

            Context.Add(_user);
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
        public async Task Handle_InvalidGetUserByIdRequest_ThrowsException()
        {
            FailedToFindException error = null;
            
            var request = new GetUserByIdRequest { Id = 100 };

            try
            {
                await Mediator.HandleAsync(request);
            }
            catch(FailedToFindException e)
            {
                error = e;
            }

            Assert.IsNotNull(error);
            Assert.IsNotNull(error.ResponseData);
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
    }

    [DoNotValidate]
    public class GetUserByIdRequest : IGetRequest<User, UserGetDto>
    {
        public int Id { get; set; }
    }
    
    public class GetByIdRequest<TEntity, TOut> : GetRequest<TEntity, int, TOut>
        where TEntity : class, IEntity
    {
        public GetByIdRequest(int id) : base(id) { }
    }

    [DoNotValidate]
    public class GetUserByNameRequest : IGetRequest<User, UserGetDto>
    {
        public string Name { get; set; }
    }

    public class GetRequestProfile<TEntity, TOut> : CrudRequestProfile<IGetRequest<TEntity, TOut>>
        where TEntity : class, IEntity
    {
        public GetRequestProfile()
        {
            ForEntity<IEntity>()
                .SelectForGetWith(request =>
                {
                    if (request is GetRequest<TEntity, int, TOut> intRequest)
                        return entity => intRequest.Data == entity.Id;

                    return null;
                });
        }
    }

    public class GetUserByIdProfile : CrudRequestProfile<GetUserByIdRequest>
    {
        public GetUserByIdProfile()
        {
            ForEntity<User>()
                .SelectForAnyWith(r => e => e.Id == r.Id)
                .UseDefault(new User { Name = "DefaultUser" });
        }
    }

    public class GetUserByNameProfile : CrudRequestProfile<GetUserByNameRequest>
    {
        public GetUserByNameProfile()
        {
            ForEntity<User>()
                .UseDefault(new User { Name = "DefaultUser" })
                .SelectForGetWith(request => entity =>
                    string.Equals(entity.Name, request.Name, StringComparison.InvariantCultureIgnoreCase));

            ConfigureErrors(config => config.FailedToFindInGetIsError = false);
        }
    }
}
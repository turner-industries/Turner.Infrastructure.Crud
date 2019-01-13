using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests.RequestTests
{
    [TestFixture]
    public class UpdateRequestTests : BaseUnitTest
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
        public async Task Handle_UpdateUserByIdRequest_UpdatesUser()
        {
            var request = new UpdateUserByIdRequest
            {
                Id = _user.Id,
                Name = "NewName"
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_user.Id, response.Data.Id);
            Assert.AreEqual("NewName", response.Data.Name);
            Assert.AreEqual("Pre/Update", response.Data.PreMessage);
            Assert.AreEqual("PostMessage/Update", response.Data.PostMessage);
        }

        [Test]
        public async Task Handle_UpdateUserByNameRequest_UpdatesUser()
        {
            var request = new UpdateUserByNameRequest
            {
                Name = _user.Name,
                Data = new UserDto { Name = "NewName" }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            var user = Context.Set<User>().FirstOrDefault();
            Assert.NotNull(user);
            Assert.AreEqual(_user.Id, user.Id);
            Assert.AreEqual("NewName", user.Name);
            Assert.AreEqual(_user.PreMessage, user.PreMessage);
            Assert.AreEqual(_user.PostMessage, user.PostMessage);
        }

        [Test]
        public async Task Handle_InvalidUpdateUserByNameRequest_ReturnsError()
        {
            var request = new UpdateUserByNameRequest { Name = "NonUser", Data = new UserDto() };
            var response = await Mediator.HandleAsync(request);
            
            Assert.IsTrue(response.HasErrors);
        }

        [Test]
        public async Task Handle_InvalidUpdateUserByIdRequest_ReturnsNull()
        {
            var request = new UpdateUserByIdRequest { Id = 100, Name = string.Empty };
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task Handle_DefaultUpdateWithoutResponseRequest_UpdatesUser()
        {
            var request = new UpdateRequest<User, UserGetDto>(new UserGetDto
            {
                Id = _user.Id,
                Name = "NewUser"
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            var user = Context.Set<User>().First();
            Assert.IsNotNull(user);
            Assert.AreEqual(_user.Id, user.Id);
            Assert.AreEqual(_user.Name, user.Name);
            Assert.AreEqual(_user.PreMessage, user.PreMessage);
            Assert.AreEqual(_user.PostMessage, user.PostMessage);
        }

        [Test]
        public async Task Handle_DefaultUpdateWithResponseRequest_UpdatesUser()
        {
            var request = new UpdateRequest<User, UserGetDto, UserGetDto>(new UserGetDto
            {
                Id = _user.Id,
                Name = "NewUser"
            });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_user.Id, response.Data.Id);
            Assert.AreEqual(_user.Name, response.Data.Name);
            Assert.AreEqual(_user.PreMessage, response.Data.PreMessage);
            Assert.AreEqual(_user.PostMessage, response.Data.PostMessage);
        }
        
        [Test]
        public async Task Handle_UpdateRequestWithBuiltSelector_UpdatesUser()
        {
            var request = new UpdateRequest<User, int, UserDto, UserGetDto>(
                _user.Id, 
                new UserDto { Name = "NewUser" });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_user.Id, response.Data.Id);
            Assert.AreEqual(_user.Name, response.Data.Name);
            Assert.AreEqual(_user.PreMessage, response.Data.PreMessage);
            Assert.AreEqual(_user.PostMessage, response.Data.PostMessage);
        }

        [Test]
        public async Task Handle_UpdateByIdRequest_UpdatesUser()
        {
            var request = new UpdateByIdRequest<User, UserDto, UserGetDto>(
                _user.Id,
                new UserDto { Name = "NewUser" });

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_user.Id, response.Data.Id);
            Assert.AreEqual("NewUser", response.Data.Name);
        }
    }
    
    [DoNotValidate]
    public class UpdateUserByIdRequest 
        : UserDto, IUpdateRequest<User, UserGetDto>
    {
        public int Id { get; set; }
    }

    [DoNotValidate]
    public class UpdateUserByNameRequest
        : IUpdateRequest<User>
    {
        public string Name { get; set; }
        public UserDto Data { get; set; }
    }

    public class UpdateRequestProfile<TEntity, TOut> 
        : CrudRequestProfile<IUpdateRequest<TEntity, TOut>>
        where TEntity : class, IEntity
    {
        public UpdateRequestProfile()
        {
            ForEntity<IEntity>()
                .WithEntityHook((request, entity) => entity.PostMessage = "PostMessage");
        }
    }

    public class UpdateUserWithoutResponseRequestProfile 
        : CrudRequestProfile<UpdateRequest<User, UserGetDto>>
    {
        public UpdateUserWithoutResponseRequestProfile()
        {
            ForEntity<User>()
                .WithKeys(r => r.Data.Id, e => e.Id);
        }
    }

    public class UpdateUserWithResponseRequestProfile 
        : CrudRequestProfile<UpdateRequest<User, UserGetDto, UserGetDto>>
    {
        public UpdateUserWithResponseRequestProfile()
        {
            ForEntity<User>()
                .SelectWith(builder => builder.Single(request => request.Data.Id, entity => entity.Id));
        }
    }

    public class UpdateUserByIdProfile 
        : CrudRequestProfile<UpdateUserByIdRequest>
    {
        public UpdateUserByIdProfile()
        {
            WithRequestHook(request => request.PreMessage += "/Update");

            ForEntity<User>()
                .SelectWith(builder => builder.Single(request => entity => entity.Id == request.Id))
                .WithEntityHook((request, entity) => entity.PostMessage += "/Update");

            ConfigureErrors(config => config.FailedToFindInUpdateIsError = false);
        }
    }

    public class UpdateUserByNameProfile 
        : CrudRequestProfile<UpdateUserByNameRequest>
    {
        public UpdateUserByNameProfile()
        {
            ForEntity<User>()
                .SelectWith((Configuration.Builders.Select.SelectorBuilder<UpdateUserByNameRequest, User> builder) => builder.Single(
                    e => e.Name, 
                    r => r.Name,
                    (e, r) => string.Equals(e, r, StringComparison.InvariantCultureIgnoreCase)))
                .UpdateWith((request, entity) =>
                    Task.FromResult(Mapper.Map(request.Data, entity)));

            ConfigureErrors(config => config.FailedToFindInUpdateIsError = true);
        }
    }
}
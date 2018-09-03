using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Errors;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests
{
    [TestFixture]
    public class DeleteRequestTests : BaseUnitTest
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
        public async Task Handle_DeleteUserByIdRequest_DeletesUser()
        {
            var request = new DeleteUserByIdRequest
            {
                Id = _user.Id
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.IsTrue(response.Data.IsDeleted);
            Assert.AreEqual("PostDelete/Entity/Delete", response.Data.PostMessage);
        }

        [Test]
        public async Task Handle_DeleteUserByNameRequest_DeletesUser()
        {
            var request = new DeleteUserByNameRequest
            {
                Name = _user.Name
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            var user = Context.Set<User>().FirstOrDefault();
            Assert.NotNull(user);
            Assert.IsTrue(user.IsDeleted);
            Assert.AreEqual("PostDelete/Entity", user.PostMessage);
        }

        [Test]
        public async Task Handle_InvalidDeleteUserByNameRequest_ThrowsException()
        {
            FailedToFindException error = null;

            var request = new DeleteUserByNameRequest { Name = "NonUser" };

            try
            {
                await Mediator.HandleAsync(request);
            }
            catch (FailedToFindException e)
            {
                error = e;
            }

            Assert.IsNotNull(error);
            Assert.IsFalse(Context.Set<User>().First().IsDeleted);
        }

        [Test]
        public async Task Handle_InvalidDeleteUserByIdRequest_ReturnsNull()
        {
            var request = new DeleteUserByIdRequest { Id = 100 };
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsFalse(Context.Set<User>().First().IsDeleted);
            Assert.IsNull(response.Data);
        }

        [Test]
        public async Task Handle_DefaultDeleteByIdWithoutResponse_DeletesUser()
        {
            var request = new DeleteByIdRequest<User>(_user.Id);

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            var user = Context.Set<User>().First();
            Assert.IsNotNull(user);
            Assert.AreEqual(_user.Id, user.Id);
            Assert.IsTrue(user.IsDeleted);
        }

        [Test]
        public async Task Handle_DefaultDeleteByIdWithResponse_DeletesUserAndReturnsDto()
        {
            var request = new DeleteByIdRequest<User, UserGetDto>(_user.Id);

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_user.Id, response.Data.Id);
            Assert.IsTrue(response.Data.IsDeleted);
        }
    }
    
    [DoNotValidate]
    public class DeleteUserByIdRequest 
        : IDeleteRequest<User, UserGetDto>
    {
        public int Id { get; set; }
    }

    [DoNotValidate]
    public class DeleteUserByNameRequest
        : IDeleteRequest<User>
    {
        public string Name { get; set; }
    }
    
    public class DeleteUserByIdProfile : CrudRequestProfile<DeleteUserByIdRequest>
    {
        public DeleteUserByIdProfile()
        {
            ForEntity<User>()
                .SelectForDeleteWith(builder => builder.Build("Id"))
                .AfterDeleting(entity => entity.PostMessage += "/Delete");

            ConfigureErrors(config => config.FailedToFindInDeleteIsError = false);
        }
    }

    public class DeleteUserByNameProfile : CrudRequestProfile<DeleteUserByNameRequest>
    {
        public DeleteUserByNameProfile()
        {
            ForEntity<User>()
                .SelectForAnyWith(builder => 
                    builder.Build(request => entity =>
                        string.Equals(entity.Name, request.Name, StringComparison.InvariantCultureIgnoreCase)));

            ConfigureErrors(config => config.FailedToFindInDeleteIsError = true);
        }
    }
}
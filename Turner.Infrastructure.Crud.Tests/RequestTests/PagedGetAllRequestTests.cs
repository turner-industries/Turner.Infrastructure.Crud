using NUnit.Framework;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests.RequestTests
{
    [TestFixture]
    public class PagedGetAllRequestTests : BaseUnitTest
    {
        private async Task SeedEntities()
        {
            await Context.AddRangeAsync(
                new User { Name = "BUser" },
                new User { Name = "AUser" },
                new User { Name = "CUser" },
                new User { Name = "FUser" },
                new User { Name = "DUser" },
                new User { Name = "EUser" }
            );

            await Context.SaveChangesAsync();
        }

        [Test]
        public async Task Handle_GetAllUsersPagedRequest_NoPageSize_ReturnsAllEntities()
        {
            await SeedEntities();

            var request = new GetAllUsersPaged
            {
                PageSize = 0
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(1, response.Data.PageNumber);
            Assert.AreEqual(6, response.Data.PageSize);
            Assert.AreEqual(1, response.Data.PageCount);
            Assert.AreEqual(6, response.Data.TotalItemCount);
            Assert.AreEqual(6, response.Data.Items.Count);
            Assert.AreEqual("FUser", response.Data.Items[0].Name);
            Assert.AreEqual("EUser", response.Data.Items[1].Name);
            Assert.AreEqual("DUser", response.Data.Items[2].Name);
            Assert.AreEqual("CUser", response.Data.Items[3].Name);
            Assert.AreEqual("BUser", response.Data.Items[4].Name);
            Assert.AreEqual("AUser", response.Data.Items[5].Name);
        }

        [Test]
        public async Task Handle_GetAllUsersPagedRequest_DefaultPage_ReturnsPagedEntities()
        {
            await SeedEntities();

            var request = new GetAllUsersPaged
            {
                PageSize = 5
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(1, response.Data.PageNumber);
            Assert.AreEqual(5, response.Data.PageSize);
            Assert.AreEqual(2, response.Data.PageCount);
            Assert.AreEqual(6, response.Data.TotalItemCount);
            Assert.AreEqual(5, response.Data.Items.Count);
            Assert.AreEqual("FUser", response.Data.Items[0].Name);
            Assert.AreEqual("EUser", response.Data.Items[1].Name);
            Assert.AreEqual("DUser", response.Data.Items[2].Name);
            Assert.AreEqual("CUser", response.Data.Items[3].Name);
            Assert.AreEqual("BUser", response.Data.Items[4].Name);
        }

        [Test]
        public async Task Handle_GetAllUsersPagedRequest_WithPageNumber_ReturnsPagedEntities()
        {
            await SeedEntities();

            var request = new GetAllUsersPaged
            {
                PageSize = 2,
                PageNumber = 2
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(2, response.Data.PageNumber);
            Assert.AreEqual(2, response.Data.PageSize);
            Assert.AreEqual(3, response.Data.PageCount);
            Assert.AreEqual(6, response.Data.TotalItemCount);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("DUser", response.Data.Items[0].Name);
            Assert.AreEqual("CUser", response.Data.Items[1].Name);
        }

        [Test]
        public async Task Handle_DefaultPagedGetAllRequest_ReturnsPagedEntities()
        {
            await SeedEntities();

            var request = new PagedGetAllRequest<User, UserGetDto>
            {
                PageSize = 2,
                PageNumber = 2
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(2, response.Data.PageNumber);
            Assert.AreEqual(2, response.Data.PageSize);
            Assert.AreEqual(3, response.Data.PageCount);
            Assert.AreEqual(6, response.Data.TotalItemCount);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("DUser", response.Data.Items[0].Name);
            Assert.AreEqual("CUser", response.Data.Items[1].Name);
        }
    }

    [DoNotValidate]
    public class GetAllUsersPaged : IPagedGetAllRequest<User, UserGetDto>
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }

    public class GetAllUsersPagedProfile : CrudRequestProfile<IPagedGetAllRequest>
    {
        public GetAllUsersPagedProfile()
        {
            ForEntity<User>().SortGetAllWith(builder => builder.SortBy(x => x.Name).Descending());
        }
    }
}
 
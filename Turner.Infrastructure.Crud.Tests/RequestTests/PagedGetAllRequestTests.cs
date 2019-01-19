using NUnit.Framework;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;

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

        [Test]
        public async Task Handle_GetAllUsersPagedRequest_WithNoResults_ReturnsEmptyList()
        {
            await Context.AddRangeAsync(
                new User { Name = "BUser", IsDeleted = true },
                new User { Name = "AUser", IsDeleted = true },
                new User { Name = "CUser", IsDeleted = true },
                new User { Name = "FUser", IsDeleted = true },
                new User { Name = "DUser", IsDeleted = true },
                new User { Name = "EUser", IsDeleted = true }
            );

            await Context.SaveChangesAsync();

            var request = new GetAllFilteredUsersPaged
            {
                PageSize = 2,
                PageNumber = 2,
                DeletedFilter = false
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(0, response.Data.TotalItemCount);
            Assert.AreEqual(1, response.Data.PageNumber);
            Assert.AreEqual(2, response.Data.PageSize);
            Assert.AreEqual(1, response.Data.PageCount);
            Assert.AreEqual(0, response.Data.Items.Count);
        }

        [Test]
        public async Task Handle_GetAllUsersPagedRequest_WithFilter_ReturnsFilteredList()
        {
            await Context.AddRangeAsync(
                new User { Name = "BUser", IsDeleted = true },
                new User { Name = "AUser", IsDeleted = false },
                new User { Name = "CUser", IsDeleted = true },
                new User { Name = "FUser", IsDeleted = false },
                new User { Name = "DUser", IsDeleted = false },
                new User { Name = "EUser", IsDeleted = true }
            );

            await Context.SaveChangesAsync();

            var request = new GetAllFilteredUsersPaged
            {
                PageSize = 2,
                PageNumber = 1,
                DeletedFilter = false
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(3, response.Data.TotalItemCount);
            Assert.AreEqual(1, response.Data.PageNumber);
            Assert.AreEqual(2, response.Data.PageSize);
            Assert.AreEqual(2, response.Data.PageCount);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("AUser", response.Data.Items[0].Name);
            Assert.AreEqual("DUser", response.Data.Items[1].Name);
        }
    }
    
    public class GetAllUsersPaged : IPagedGetAllRequest<User, UserGetDto>
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }

    public class GetAllUsersPagedProfile 
        : CrudRequestProfile<IPagedGetAllRequest>
    {
        public GetAllUsersPagedProfile()
        {
            ForEntity<User>().SortWith(builder => builder.SortBy(x => x.Name).Descending());
        }
    }
    
    public class GetAllFilteredUsersPaged
        : IPagedGetAllRequest<User, UserGetDto>
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public bool? DeletedFilter { get; set; }
    }

    public class GetAllFilteredUsersPagedProfile 
        : CrudRequestProfile<GetAllFilteredUsersPaged>
    {
        public GetAllFilteredUsersPagedProfile()
        {
            ConfigureErrors(config => config.FailedToFindInGetAllIsError = false);

            ForEntity<User>()
                .FilterWith(builder => builder
                    .FilterOn(request => entity => entity.IsDeleted == request.DeletedFilter.Value)
                    .When(r => r.DeletedFilter.HasValue))
                .SortWith(builder => builder.SortBy("Name"));
        }
    }
}

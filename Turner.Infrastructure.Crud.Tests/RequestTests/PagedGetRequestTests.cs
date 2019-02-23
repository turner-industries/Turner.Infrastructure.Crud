using NUnit.Framework;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;

namespace Turner.Infrastructure.Crud.Tests.RequestTests
{
    [TestFixture]
    public class PagedGetRequestTests : BaseUnitTest
    {
        private async Task SeedEntities()
        {
            Context.AddRange(
                new User { Name = "CUser", IsDeleted = false },
                new User { Name = "BUser", IsDeleted = true },
                new User { Name = "FUser", IsDeleted = false },
                new User { Name = "AUser", IsDeleted = false },
                new User { Name = "EUser", IsDeleted = false },
                new User { Name = "DUser", IsDeleted = false }
            );

            await Context.SaveChangesAsync();
        }

        [Test]
        public async Task Handle_PagedGetRequest_ReturnsItemPageAndPageInfo()
        {
            await SeedEntities();
            
            await Context.SaveChangesAsync();

            var request = new PagedGetUserRequest
            {
                Name = "CUser",
                PageSize = 2
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(5, response.Data.TotalItemCount);
            Assert.AreEqual(2, response.Data.PageNumber);
            Assert.AreEqual(2, response.Data.PageSize);
            Assert.AreEqual(3, response.Data.PageCount);
            Assert.AreEqual("DUser", response.Data.Items[0].Name);
            Assert.AreEqual("CUser", response.Data.Items[1].Name);
        }

        [Test]
        public async Task Handle_DefaultPagedGetRequest_ReturnsItemPageAndPageInfo()
        {
            await SeedEntities();

            await Context.SaveChangesAsync();

            var request = new PagedGetByIdRequest<User, UserGetDto>(4, 2);
            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.IsNotNull(response.Data);
            Assert.IsNotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(6, response.Data.TotalItemCount);
            Assert.AreEqual(2, response.Data.PageNumber);
            Assert.AreEqual(2, response.Data.PageSize);
            Assert.AreEqual(3, response.Data.PageCount);
            Assert.AreEqual("FUser", response.Data.Items[0].Name);
            Assert.AreEqual("AUser", response.Data.Items[1].Name);
        }
    }
    
    public class PagedGetUserRequest : IPagedGetRequest<User, UserGetDto>
    {
        public string Name { get; set; }

        public int PageSize { get; set; }
    }

    public class PagedGetUserProfile 
        : CrudRequestProfile<PagedGetUserRequest>
    {
        public PagedGetUserProfile()
        {
            ForEntity<User>()
                .SelectWith(builder => builder.Single("Name"))
                .SortWith(builder => builder.SortBy("Name").Descending())
                .FilterWith(builder => builder.Using(x => !x.IsDeleted))
                .ConfigureOptions(config => config.UseProjection = false);
        }
    }
}

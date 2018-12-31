﻿using NUnit.Framework;
using System.Threading.Tasks;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests.RequestTests
{
    [TestFixture]
    public class PagedGetRequestTests : BaseUnitTest
    {
        private async Task SeedEntities()
        {
            await Context.AddRangeAsync(
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

            var request = new PagedGetUser
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
    }

    [DoNotValidate]
    public class PagedGetUser : IPagedGetRequest<User, UserGetDto>
    {
        public string Name { get; set; }

        public int PageSize { get; set; }
    }

    public class PagedGetUserProfile : CrudRequestProfile<IPagedGetRequest>
    {
        public PagedGetUserProfile()
        {
            ForEntity<User>()
                .SelectWith(builder => builder.Single("Name"))
                .SortWith(builder => builder.SortBy("Name").Descending())
                .FilterWith(builder => builder.FilterOn(x => !x.IsDeleted))
                .ConfigureOptions(config => config.UseProjection = false);
        }
    }
}
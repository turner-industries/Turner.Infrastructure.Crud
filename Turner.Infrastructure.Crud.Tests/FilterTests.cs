using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;

namespace Turner.Infrastructure.Crud.Tests
{
    [TestFixture]
    public class FilterTests : BaseUnitTest
    {
        private DateTime _date = DateTime.Now; // TODO: Make this a specific date

        [SetUp]
        public async Task SetUp()
        {
            Context.AddRange(
                new Site { Guid = Guid.NewGuid(), CreatedDate = _date.AddDays(-2) },
                new Site { Guid = Guid.NewGuid(), CreatedDate = _date },
                new Site { Guid = Guid.NewGuid(), CreatedDate = _date },
                new Site { Guid = Guid.NewGuid(), CreatedDate = _date.AddDays(2) }
            );

            await Context.SaveChangesAsync();
        }

        [Test]
        public async Task Handle_TestExclusiveLessConstantDateFilter_FiltersEntities()
        {
            var request = new FilterExclusiveLessConstantDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(_date.AddDays(-2), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(_date, response.Data.Items[1].CreatedDate);
            Assert.AreEqual(_date, response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_TestExclusiveLessDateFilter_FiltersEntities()
        {
            var request = new FilterExclusiveLessDateRequest
            {
                Date = _date
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(1, response.Data.Items.Count);
            Assert.AreEqual(_date.AddDays(-2), response.Data.Items[0].CreatedDate);
        }
    }

    public class FilterExclusiveLessConstantDateRequest 
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class FilterExclusiveLessDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime Date { get; set; }
    }

    public class FilterExclusiveLessConstantDateProfile
        : CrudRequestProfile<FilterExclusiveLessConstantDateRequest>
    {
        public FilterExclusiveLessConstantDateProfile()
        {
            ForEntity<Site>()
                .SearchBefore(x => x.CreatedDate, DateTime.Now.AddDays(1), true);
        }
    }

    public class FilterExclusiveLessDateProfile
        : CrudRequestProfile<FilterExclusiveLessDateRequest>
    {
        public FilterExclusiveLessDateProfile()
        {
            ForEntity<Site>()
                .SearchBefore(request => request.Date, entity => entity.CreatedDate, true);
        }
    }
}
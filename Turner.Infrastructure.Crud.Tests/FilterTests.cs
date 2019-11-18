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
        public static DateTime Date = new DateTime(2020, 2, 2, 12, 0, 0, DateTimeKind.Utc);

        [SetUp]
        public async Task SetUp()
        {
            Context.AddRange(
                new Site { Name = "BADSite1", Guid = Guid.NewGuid(), CreatedDate = Date.AddDays(-4) },
                new Site { Name = "BADSite2", Guid = Guid.NewGuid(), CreatedDate = Date.AddDays(-2) },
                new Site { Name = "Site3BAD", Guid = Guid.NewGuid(), CreatedDate = Date },
                new Site { Name = "SiBDAte4BAD", Guid = Guid.NewGuid(), CreatedDate = Date.AddDays(2) },
                new Site { Name = "SiBDAte5", Guid = Guid.NewGuid(), CreatedDate = Date.AddDays(4) }
            );

            await Context.SaveChangesAsync();
        }

        [Test]
        public async Task Handle_IncludeBeforeConstantDate_FiltersEntities()
        {
            var request = new IncludeBeforeConstantDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-4), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date, response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeBeforeRequestDate_FiltersEntities()
        {
            var request = new IncludeBeforeDateRequest { Date = Date };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-4), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_ExcludeBeforeConstantDate_FiltersEntities()
        {
            var request = new ExcludeBeforeConstantDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(4), response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_ExcludeBeforeRequestDate_FiltersEntities()
        {
            var request = new ExcludeBeforeDateRequest { Date = Date };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date, response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date.AddDays(4), response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeAfterConstantDate_FiltersEntities()
        {
            var request = new IncludeAfterConstantDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date, response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date.AddDays(4), response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeAfterRequestDate_FiltersEntities()
        {
            var request = new IncludeAfterDateRequest { Date = Date };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(4), response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_ExcludeAfterConstantDate_FiltersEntities()
        {
            var request = new ExcludeAfterConstantDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-4), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[1].CreatedDate);
        }

        [Test]
        public async Task Handle_ExcludeAfterRequestDate_FiltersEntities()
        {
            var request = new ExcludeAfterDateRequest { Date = Date };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-4), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date, response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeWithinConstantAndConstantDate_FiltersEntities()
        {
            var request = new IncludeWithinConstantAndConstantDateRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date, response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[2].CreatedDate);
        }

        [Test]
        public async Task Handle_IncludeWithinRequestAndConstantDate_FiltersEntities()
        {
            var request = new IncludeWithinRequestAndConstantDateRequest
            {
                MinDate = Date.AddDays(-3)
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(3, response.Data.Items.Count);
            Assert.AreEqual(Date.AddDays(-2), response.Data.Items[0].CreatedDate);
            Assert.AreEqual(Date, response.Data.Items[1].CreatedDate);
            Assert.AreEqual(Date.AddDays(2), response.Data.Items[2].CreatedDate);
        }
        
        [Test]
        public async Task Handle_IncludeStartsWithConstantString_FiltersEntities()
        {
            var request = new IncludeStartsWithConstantStringRequest();

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("BADSite1", response.Data.Items[0].Name);
            Assert.AreEqual("BADSite2", response.Data.Items[1].Name);
        }

        [Test]
        public async Task Handle_IncludeStartsWithString_FiltersEntities()
        {
            var request = new IncludeStartsWithStringRequest { Name = "BAD" };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(2, response.Data.Items.Count);
            Assert.AreEqual("BADSite1", response.Data.Items[0].Name);
            Assert.AreEqual("BADSite2", response.Data.Items[1].Name);
        }

        [Test]
        public async Task Handle_ExcludeWithFalsePredicate_DoesNotFilterEntities()
        {
            var request = new ExcludeWithPredicateRequest { Name = string.Empty };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(5, response.Data.Items.Count);
        }

        [Test]
        public async Task Handle_ExcludeWithTruePredicate_FiltersEntities()
        {
            var request = new ExcludeWithPredicateRequest { Name = "BAD" };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Items);
            Assert.AreEqual(1, response.Data.Items.Count);
        }
    }

    public class IncludeBeforeConstantDateRequest 
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class IncludeBeforeConstantDateProfile
        : RequestProfile<IncludeBeforeConstantDateRequest>
    {
        public IncludeBeforeConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterUsing(e => e.CreatedDate < FilterTests.Date.AddDays(1).Date);
        }
    }

    public class IncludeBeforeDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime Date { get; set; }
    }

    public class IncludeBeforeDateProfile
        : RequestProfile<IncludeBeforeDateRequest>
    {
        public IncludeBeforeDateProfile()
        {
            ForEntity<Site>()
                .FilterUsing((r, e) => e.CreatedDate < r.Date);
        }
    }

    public class ExcludeBeforeConstantDateRequest
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class ExcludeBeforeConstantDateProfile
        : RequestProfile<ExcludeBeforeConstantDateRequest>
    {
        public ExcludeBeforeConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterUsing(e => e.CreatedDate > FilterTests.Date.AddDays(1).Date);
        }
    }

    public class ExcludeBeforeDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime Date { get; set; }
    }

    public class ExcludeBeforeDateProfile
        : RequestProfile<ExcludeBeforeDateRequest>
    {
        public ExcludeBeforeDateProfile()
        {
            ForEntity<Site>()
                .FilterUsing((r, e) => e.CreatedDate >= r.Date);
        }
    }

    public class IncludeAfterConstantDateRequest
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class IncludeAfterConstantDateProfile
        : RequestProfile<IncludeAfterConstantDateRequest>
    {
        public IncludeAfterConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterUsing(e => e.CreatedDate > FilterTests.Date.AddDays(-1).Date);
        }
    }

    public class IncludeAfterDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime Date { get; set; }
    }

    public class IncludeAfterDateProfile
        : RequestProfile<IncludeAfterDateRequest>
    {
        public IncludeAfterDateProfile()
        {
            ForEntity<Site>()
                .FilterUsing((r, e) => e.CreatedDate > r.Date);
        }
    }

    public class ExcludeAfterConstantDateRequest
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class ExcludeAfterConstantDateProfile
        : RequestProfile<ExcludeAfterConstantDateRequest>
    {
        public ExcludeAfterConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterUsing(e => e.CreatedDate <= FilterTests.Date.AddDays(-1).Date);
        }
    }

    public class ExcludeAfterDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime Date { get; set; }
    }

    public class ExcludeAfterDateProfile
        : RequestProfile<ExcludeAfterDateRequest>
    {
        public ExcludeAfterDateProfile()
        {
            ForEntity<Site>()
                .FilterUsing((r, e) => e.CreatedDate <= r.Date);
        }
    }

    public class IncludeWithinConstantAndConstantDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
    }

    public class IncludeWithinConstantAndConstantDateProfile
        : RequestProfile<IncludeWithinConstantAndConstantDateRequest>
    {
        public IncludeWithinConstantAndConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterUsing(e => e.CreatedDate > FilterTests.Date.AddDays(-3) && e.CreatedDate < FilterTests.Date.AddDays(3));
        }
    }

    public class IncludeWithinRequestAndConstantDateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public DateTime MinDate { get; set; }
    }

    public class IncludeWithinRequestAndConstantDateProfile
        : RequestProfile<IncludeWithinRequestAndConstantDateRequest>
    {
        public IncludeWithinRequestAndConstantDateProfile()
        {
            ForEntity<Site>()
                .FilterUsing((r, e) => e.CreatedDate > r.MinDate && e.CreatedDate < FilterTests.Date.AddDays(3));
        }
    }
    
    public class IncludeStartsWithConstantStringRequest
        : GetAllRequest<Site, SiteGetDto>
    { }

    public class IncludeStartsWithConstantStringProfile
        : RequestProfile<IncludeStartsWithConstantStringRequest>
    {
        public IncludeStartsWithConstantStringProfile()
        {
            ForEntity<Site>()
                .FilterUsing(e => e.Name.StartsWith("BAD"));
        }
    }

    public class IncludeStartsWithStringRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public string Name { get; set; }
    }

    public class IncludeStartsWithStringProfile
        : RequestProfile<IncludeStartsWithStringRequest>
    {
        public IncludeStartsWithStringProfile()
        {
            ForEntity<Site>()
                .FilterUsing((r, e) => e.Name.StartsWith(r.Name));
        }
    }
    
    public class ExcludeWithPredicateRequest
        : GetAllRequest<Site, SiteGetDto>
    {
        public string Name { get; set; }
    }

    public class ExcludeWithPredicateProfile
        : RequestProfile<ExcludeWithPredicateRequest>
    {
        public ExcludeWithPredicateProfile()
        {
            ForEntity<Site>()
                .FilterUsing(r => !string.IsNullOrEmpty(r.Name), (r, e) => !e.Name.Contains(r.Name));
        }
    }
}
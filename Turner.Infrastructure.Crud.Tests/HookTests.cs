using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Turner.Infrastructure.Crud.Configuration;
using Turner.Infrastructure.Crud.Requests;
using Turner.Infrastructure.Crud.Tests.Fakes;
using Turner.Infrastructure.Mediator.Decorators;

namespace Turner.Infrastructure.Crud.Tests
{
    [TestFixture]
    public class HookTests : BaseUnitTest
    {
        [Test]
        public async Task Handle_TestHooks_RunsAllHooksInCorrectOrder()
        {
            var request = new TestHooksRequest
            {
                Items = new List<HookDto>
                {
                    new HookDto(),
                    new HookDto()
                }
            };

            var response = await Mediator.HandleAsync(request);

            Assert.IsFalse(response.HasErrors);
            Assert.AreEqual(2, Context.Set<HookEntity>().Count());

            var entity = Context.Set<HookEntity>().First();
            Assert.AreEqual("r1/r2/r3/r4/r5", entity.RequestHookMessage);
            Assert.AreEqual("e1/e2/e3/e4/e5", entity.EntityHookMessage);
            Assert.AreEqual("i1/i2/i3/i4/i5", entity.ItemHookMessage);
            Assert.AreEqual("t1/t2/t3/t4/t5", response.Data.Items[0]);
        }
    }
    
    public class HookDto
    {
        public int Id { get; set; }

        public string RequestHookMessage { get; set; }

        public string EntityHookMessage { get; set; }

        public string ItemHookMessage { get; set; }
    }
    
    public interface ITestHooksRequest : ICreateAllRequest<HookEntity, string>
    {
        List<HookDto> Items { get; set; }
    }

    [DoNotValidate]
    public class TestHooksRequest : ITestHooksRequest
    {
        public List<HookDto> Items { get; set; } = new List<HookDto>();
    }
    
    public class TestHooksProfile
        : CrudBulkRequestProfile<ITestHooksRequest, HookDto>
    {
        public TestHooksProfile() : base(r => r.Items)
        {
            AddRequestHook(r => r.Items.ForEach(i => i.RequestHookMessage = "r1/"));
            AddRequestHook(r =>
            {
                r.Items.ForEach(i => i.RequestHookMessage += "r2/");
                return Task.CompletedTask;
            });

            AddResultHook<CreateAllResult<string>>((r, t) =>
            {
                return new CreateAllResult<string>(t.Items.Select(x => "t1/"));
            });

            AddResultHook<string>((r, t) => Task.FromResult(t + "t2/"));

            ForEntity<IHookEntity>()
                .AddEntityHook((r, e) => e.EntityHookMessage = "e1/")
                .AddEntityHook((r, e) =>
                {
                    e.EntityHookMessage += "e2/";
                    return Task.CompletedTask;
                })
                .AddItemHook((r, i) =>
                {
                    i.ItemHookMessage += "i1/";
                    return i;
                })
                .AddItemHook((r, i) =>
                {
                    i.ItemHookMessage += "i2/";
                    return Task.FromResult(i);
                });
        }
    }

    public class TestHooksRequestProfile
        : CrudBulkRequestProfile<TestHooksRequest, HookDto>
    {
        public TestHooksRequestProfile() : base(r => r.Items)
        {
            AddRequestHook(new TestInstanceRequestHook());
            AddRequestHook<TestTypeRequestHook>();
            AddRequestHook(new TestContravariantRequestHook());

            AddResultHook(new TestInstanceResultHook());
            AddResultHook<TestTypeResultHook, string>();
            AddResultHook(new TestContravariantResultHook());

            ForEntity<HookEntity>()
                .CreateResultWith(x => string.Empty)
                .AddEntityHook(new TestInstanceEntityHook())
                .AddEntityHook<TestTypeEntityHook>()
                .AddEntityHook(new TestContravariantEntityHook())
                .AddItemHook(new TestInstanceItemHook())
                .AddItemHook<TestTypeItemHook>()
                .AddItemHook(new TestContravariantItemHook());
        }
    }
}
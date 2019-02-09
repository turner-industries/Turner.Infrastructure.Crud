using System;
using System.Collections.Generic;
using System.Linq;

namespace Turner.Infrastructure.Crud.Configuration
{
    public abstract class HookConfig<TRequest, THookFactory, THook>
    {
        private readonly Func<THookFactory, THook> _hookFactoryCreateFunc;

        private List<THookFactory> _hookFactories = new List<THookFactory>();

        protected HookConfig(Func<THookFactory, THook> hookFactoryCreateFunc)
        {
            _hookFactoryCreateFunc = hookFactoryCreateFunc;
        }

        internal void SetHooks(List<THookFactory> hookFactories)
        {
            _hookFactories = hookFactories;
        }

        internal void AddHooks(IEnumerable<THookFactory> hookFactories)
        {
            _hookFactories.AddRange(hookFactories);
        }

        public List<THook> GetHooks(TRequest request)
            => _hookFactories.Select(_hookFactoryCreateFunc).ToList();
    }

    public class RequestHookConfig<TRequest> 
        : HookConfig<TRequest, IRequestHookFactory, IBoxedRequestHook>
    {
        public RequestHookConfig() : base(factory => factory.Create())
        {
        }
    }

    public class EntityHookConfig<TRequest>
        : HookConfig<TRequest, IEntityHookFactory, IBoxedEntityHook>
    {
        public EntityHookConfig() : base(factory => factory.Create())
        {
        }
    }

    public class ItemHookConfig<TRequest>
        : HookConfig<TRequest, IItemHookFactory, IBoxedItemHook>
    {
        public ItemHookConfig() : base(factory => factory.Create())
        {
        }
    }

    public class ResultHookConfig<TRequest>
        : HookConfig<TRequest, IResultHookFactory, IBoxedResultHook>
    {
        public ResultHookConfig() : base(factory => factory.Create())
        {
        }
    }
}

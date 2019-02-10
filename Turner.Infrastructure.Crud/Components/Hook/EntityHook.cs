using System;
using System.Threading;
using System.Threading.Tasks;

namespace Turner.Infrastructure.Crud
{
    public interface IBoxedEntityHook
    {
        Task Run(object request, object entity, CancellationToken ct = default(CancellationToken));
    }

    public interface IEntityHookFactory
    {
        IBoxedEntityHook Create();
    }

    public class FunctionEntityHook
       : IBoxedEntityHook
    {
        private readonly Func<object, object, CancellationToken, Task> _hookFunc;

        public FunctionEntityHook(Func<object, object, CancellationToken, Task> hookFunc)
        {
            _hookFunc = hookFunc;
        }

        public Task Run(object request, object entity, CancellationToken ct = default(CancellationToken)) 
            => _hookFunc(request, entity, ct);
    }

    public class FunctionEntityHookFactory : IEntityHookFactory
    {
        private readonly IBoxedEntityHook _hook;

        private FunctionEntityHookFactory(Func<object, object, CancellationToken, Task> hook)
        {
            _hook = new FunctionEntityHook(hook);
        }

        internal static FunctionEntityHookFactory From<TRequest, TEntity>(
            Func<TRequest, TEntity, CancellationToken, Task> hook)
            where TEntity : class
        {
            return new FunctionEntityHookFactory(
                (request, entity, ct) => hook((TRequest)request, (TEntity)entity, ct));
        }

        internal static FunctionEntityHookFactory From<TRequest, TEntity>(
            Action<TRequest, TEntity> hook)
            where TEntity : class
        {
            return new FunctionEntityHookFactory(
                (request, entity, ct) =>
                {
                    if (ct.IsCancellationRequested)
                        return Task.FromCanceled(ct);

                    hook((TRequest)request, (TEntity)entity);

                    return Task.CompletedTask;
                });
        }

        public IBoxedEntityHook Create() => _hook;
    }

    public class InstanceEntityHookFactory : IEntityHookFactory
    {
        private readonly object _instance;
        private IBoxedEntityHook _adaptedInstance;

        private InstanceEntityHookFactory(object instance, IBoxedEntityHook adaptedInstance)
        {
            _instance = instance;
            _adaptedInstance = adaptedInstance;
        }

        internal static InstanceEntityHookFactory From<TRequest, TEntity>(
            IEntityHook<TRequest, TEntity> hook)
            where TEntity : class
        {
            return new InstanceEntityHookFactory(
                hook,
                new FunctionEntityHook((request, entity, ct) 
                    => hook.Run((TRequest)request, (TEntity)entity, ct)));
        }

        public IBoxedEntityHook Create() => _adaptedInstance;
    }

    public class TypeEntityHookFactory : IEntityHookFactory
    {
        private static Func<Type, object> s_serviceFactory;

        private Func<IBoxedEntityHook> _hookFactory;

        public TypeEntityHookFactory(Func<IBoxedEntityHook> hookFactory)
        {
            _hookFactory = hookFactory;
        }

        internal static void BindContainer(Func<Type, object> serviceFactory)
        {
            s_serviceFactory = serviceFactory;
        }

        internal static TypeEntityHookFactory From<THook, TRequest, TEntity>()
            where TEntity : class
            where THook : IEntityHook<TRequest, TEntity>
        {
            return new TypeEntityHookFactory(
                () =>
                {
                    var instance = (IEntityHook<TRequest, TEntity>)s_serviceFactory(typeof(THook));
                    return new FunctionEntityHook((request, entity, ct) 
                        => instance.Run((TRequest)request, (TEntity)entity, ct));
                });
        }
        
        public IBoxedEntityHook Create() => _hookFactory();
    }
}

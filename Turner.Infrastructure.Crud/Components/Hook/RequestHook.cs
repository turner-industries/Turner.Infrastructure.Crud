using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Turner.Infrastructure.Crud
{
    public interface IBoxedRequestHook
    {
        Task Run(object request, CancellationToken ct = default(CancellationToken));
    }

    public interface IRequestHookFactory
    {
        IBoxedRequestHook Create();
    }

    public class FunctionRequestHook
        : IBoxedRequestHook
    {
        private readonly Func<object, CancellationToken, Task> _hookFunc;

        public FunctionRequestHook(Func<object, CancellationToken, Task> hookFunc)
        {
            _hookFunc = hookFunc;
        }
        
        public Task Run(object request, CancellationToken ct = default(CancellationToken)) => _hookFunc(request, ct);
    }

    public class FunctionRequestHookFactory : IRequestHookFactory
    {
        private readonly IBoxedRequestHook _hook;

        private FunctionRequestHookFactory(Func<object, CancellationToken, Task> hook)
        {
            _hook = new FunctionRequestHook(hook);
        }

        internal static FunctionRequestHookFactory From<TRequest>(
            Func<TRequest, CancellationToken, Task> hook)
        {
            return new FunctionRequestHookFactory(
                (request, ct) => hook((TRequest)request, ct));
        }

        internal static FunctionRequestHookFactory From<TRequest>(
            Action<TRequest> hook)
        {
            return new FunctionRequestHookFactory(
                (request, ct) =>
                {
                    if (ct.IsCancellationRequested)
                        return Task.FromCanceled(ct);

                    hook((TRequest)request);

                    return Task.CompletedTask;
                });
        }

        public IBoxedRequestHook Create() => _hook;
    }

    public class InstanceRequestHookFactory : IRequestHookFactory
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly object _instance;
        private readonly IBoxedRequestHook _adaptedInstance;

        private InstanceRequestHookFactory(object instance, IBoxedRequestHook adaptedInstance)
        {
            _instance = instance;
            _adaptedInstance = adaptedInstance;
        }

        internal static InstanceRequestHookFactory From<TRequest>(
            IRequestHook<TRequest> hook)
        {
            return new InstanceRequestHookFactory(
                hook,
                new FunctionRequestHook((request, ct) => hook.Run((TRequest)request, ct)));
        }

        public IBoxedRequestHook Create() => _adaptedInstance;
    }

    public class TypeRequestHookFactory : IRequestHookFactory
    {
        private static Func<Type, object> _serviceFactory;

        private readonly Func<IBoxedRequestHook> _hookFactory;

        public TypeRequestHookFactory(Func<IBoxedRequestHook> hookFactory)
        {
            _hookFactory = hookFactory;
        }

        internal static void BindContainer(Func<Type, object> serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        internal static TypeRequestHookFactory From<THook, TRequest>()
            where THook : IRequestHook<TRequest>
        {
            return new TypeRequestHookFactory(
                () =>
                {
                    var instance = (IRequestHook<TRequest>)_serviceFactory(typeof(THook));
                    return new FunctionRequestHook((request, ct) => instance.Run((TRequest)request, ct));
                });
        }
        
        public IBoxedRequestHook Create() => _hookFactory();
    }
}

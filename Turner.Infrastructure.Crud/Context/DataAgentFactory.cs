using System;

namespace Turner.Infrastructure.Crud.Context
{
    public interface IDataAgentFactory
    {
        ICreateDataAgent GetCreateDataAgent();

        IUpdateDataAgent GetUpdateDataAgent();

        IDeleteDataAgent GetDeleteDataAgent();

        IBulkCreateDataAgent GetBulkCreateDataAgent();

        IBulkUpdateDataAgent GetBulkUpdateDataAgent();

        IBulkDeleteDataAgent GetBulkDeleteDataAgent();
    }

    public class DataAgentFactory : IDataAgentFactory
    {
        private static Func<Type, object> _serviceFactory;

        internal static void BindContainer(Func<Type, object> serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        internal static T Create<T>() => (T)_serviceFactory(typeof(T));

        public IBulkCreateDataAgent GetBulkCreateDataAgent() => Create<IBulkCreateDataAgent>();

        public IBulkDeleteDataAgent GetBulkDeleteDataAgent() => Create<IBulkDeleteDataAgent>();

        public IBulkUpdateDataAgent GetBulkUpdateDataAgent() => Create<IBulkUpdateDataAgent>();

        public ICreateDataAgent GetCreateDataAgent() => Create<ICreateDataAgent>();

        public IDeleteDataAgent GetDeleteDataAgent() => Create<IDeleteDataAgent>();

        public IUpdateDataAgent GetUpdateDataAgent() => Create<IUpdateDataAgent>();
    }
}

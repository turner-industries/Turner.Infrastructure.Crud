using System;

namespace Turner.Infrastructure.Crud
{
    public interface IRequestData
    {
        Func<object, object> DataSource { get; }

        Type DataType { get; }
    }

    public class RequestData : IRequestData
    {
        public Func<object, object> DataSource { get; private set; }

        public Type DataType { get; private set; }

        internal static RequestData From<TRequest, TData>(Func<TRequest, TData> dataSource)
        {
            var data = new RequestData();
            data.Bind(dataSource);
            return data;
        }

        internal void Bind<TRequest, TData>(Func<TRequest, TData> dataSource)
        {
            DataSource = request => dataSource((TRequest)request);
            DataType = typeof(TData);
        }
    }
}

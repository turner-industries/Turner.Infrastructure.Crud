using System;

namespace Turner.Infrastructure.Crud
{
    public interface IRequestItemSource
    {
        Func<object, object> ItemSource { get; }

        Type ItemType { get; }
    }

    public class RequestItemSource : IRequestItemSource
    {
        public Func<object, object> ItemSource { get; private set; }

        public Type ItemType { get; private set; }

        internal static RequestItemSource From<TRequest, TItem>(Func<TRequest, TItem> itemSource)
        {
            var item = new RequestItemSource();
            item.Bind(itemSource);
            return item;
        }

        private void Bind<TRequest, TItem>(Func<TRequest, TItem> itemSource)
        {
            ItemSource = request => itemSource((TRequest)request);
            ItemType = typeof(TItem);
        }
    }
}

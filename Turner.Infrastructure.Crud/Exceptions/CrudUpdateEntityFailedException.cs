using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Newtonsoft.Json;

namespace Turner.Infrastructure.Crud.Exceptions
{
    [Serializable]
    public class CrudUpdateEntityFailedException : Exception
    {
        public object ItemProperty { get; set; }

        public object EntityProperty { get; set; }

        public CrudUpdateEntityFailedException()
        {
        }

        public CrudUpdateEntityFailedException(string message)
            : base(message)
        {
        }

        public CrudUpdateEntityFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CrudUpdateEntityFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ItemProperty = JsonConvert.DeserializeObject(info.GetString(nameof(ItemProperty)));
            EntityProperty = JsonConvert.DeserializeObject(info.GetString(nameof(EntityProperty)));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(ItemProperty), JsonConvert.SerializeObject(ItemProperty));
            info.AddValue(nameof(EntityProperty), JsonConvert.SerializeObject(EntityProperty));

            base.GetObjectData(info, context);
        }
    }
}

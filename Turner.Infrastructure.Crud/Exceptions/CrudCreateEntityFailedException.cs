using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Newtonsoft.Json;

namespace Turner.Infrastructure.Crud.Exceptions
{
    [Serializable]
    public class CrudCreateEntityFailedException : Exception
    {
        public object ItemProperty { get; set; }

        public CrudCreateEntityFailedException()
        {
        }

        public CrudCreateEntityFailedException(string message)
            : base(message)
        {
        }

        public CrudCreateEntityFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CrudCreateEntityFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ItemProperty = JsonConvert.DeserializeObject(info.GetString(nameof(ItemProperty)));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            
            info.AddValue(nameof(ItemProperty), JsonConvert.SerializeObject(ItemProperty));

            base.GetObjectData(info, context);
        }
    }
}

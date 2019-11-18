using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Newtonsoft.Json;

namespace Turner.Infrastructure.Crud.Exceptions
{
    [Serializable]
    public class CreateEntityFailedException : Exception
    {
        public object ItemProperty { get; set; }

        public CreateEntityFailedException()
        {
        }

        public CreateEntityFailedException(string message)
            : base(message)
        {
        }

        public CreateEntityFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CreateEntityFailedException(SerializationInfo info, StreamingContext context)
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

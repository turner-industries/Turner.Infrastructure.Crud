using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Newtonsoft.Json;

namespace Turner.Infrastructure.Crud.Exceptions
{
    [Serializable]
    public class CreateResultFailedException : Exception
    {
        public object EntityProperty { get; set; }

        public CreateResultFailedException()
        {
        }

        public CreateResultFailedException(string message)
            : base(message)
        {
        }

        public CreateResultFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CreateResultFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            EntityProperty = JsonConvert.DeserializeObject(info.GetString(nameof(EntityProperty)));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            
            info.AddValue(nameof(EntityProperty), JsonConvert.SerializeObject(EntityProperty));

            base.GetObjectData(info, context);
        }
    }
}

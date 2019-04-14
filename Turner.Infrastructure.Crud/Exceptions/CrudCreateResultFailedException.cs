using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Newtonsoft.Json;

namespace Turner.Infrastructure.Crud.Exceptions
{
    [Serializable]
    public class CrudCreateResultFailedException : Exception
    {
        public object EntityProperty { get; set; }

        public CrudCreateResultFailedException()
        {
        }

        public CrudCreateResultFailedException(string message)
            : base(message)
        {
        }

        public CrudCreateResultFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CrudCreateResultFailedException(SerializationInfo info, StreamingContext context)
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

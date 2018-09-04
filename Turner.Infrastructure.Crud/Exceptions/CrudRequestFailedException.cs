using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Turner.Infrastructure.Crud.Exceptions
{
    [Serializable]
    public class CrudRequestFailedException : Exception
    {
        public Type RequestTypeProperty { get; set; }
        public object ResponseData { get; set; }
        
        public CrudRequestFailedException()
        {
        }

        public CrudRequestFailedException(string message)
            : base(message)
        {
        }

        public CrudRequestFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CrudRequestFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            RequestTypeProperty = (Type) info.GetValue(nameof(RequestTypeProperty), typeof(Type));
            ResponseData = JsonConvert.DeserializeObject(info.GetString(nameof(ResponseData)));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(RequestTypeProperty), RequestTypeProperty);
            info.AddValue(nameof(ResponseData), JsonConvert.SerializeObject(ResponseData));

            base.GetObjectData(info, context);
        }
    }
}

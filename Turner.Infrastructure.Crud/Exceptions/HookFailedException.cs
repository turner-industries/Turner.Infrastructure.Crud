using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Newtonsoft.Json;

namespace Turner.Infrastructure.Crud.Exceptions
{
    [Serializable]
    public class HookFailedException : Exception
    {
        public Type RequestTypeProperty { get; set; }

        public object HookProperty { get; set; }

        public HookFailedException()
        {
        }

        public HookFailedException(string message)
            : base(message)
        {
        }

        public HookFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected HookFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            RequestTypeProperty = (Type)info.GetValue(nameof(RequestTypeProperty), typeof(Type));
            HookProperty = JsonConvert.DeserializeObject(info.GetString(nameof(HookProperty)));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(RequestTypeProperty), RequestTypeProperty);
            info.AddValue(nameof(HookProperty), JsonConvert.SerializeObject(HookProperty));

            base.GetObjectData(info, context);
        }
    }
}

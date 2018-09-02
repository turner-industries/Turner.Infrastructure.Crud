using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Turner.Infrastructure.Crud.Errors
{
    [Serializable]
    public class FailedToFindException : CrudRequestFailedException
    {
        public Type QueryTypeProperty { get; set; }

        public FailedToFindException() 
            : base()
        {
        }

        public FailedToFindException(string message)
            : base(message)
        {
        }

        public FailedToFindException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected FailedToFindException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            QueryTypeProperty = (Type) info.GetValue(nameof(QueryTypeProperty), typeof(Type));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(QueryTypeProperty), QueryTypeProperty);

            base.GetObjectData(info, context);
        }

    }
}

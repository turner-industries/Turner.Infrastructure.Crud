using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Turner.Infrastructure.Crud.Exceptions
{
    [Serializable]
    public class FailedToFindException : Exception
    {
        public Type EntityTypeProperty { get; set; }

        public FailedToFindException()
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
            EntityTypeProperty = (Type)info.GetValue(nameof(EntityTypeProperty), typeof(Type));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(EntityTypeProperty), EntityTypeProperty);

            base.GetObjectData(info, context);
        }
    }
}

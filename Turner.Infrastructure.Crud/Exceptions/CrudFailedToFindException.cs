using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Turner.Infrastructure.Crud.Exceptions
{
    [Serializable]
    public class CrudFailedToFindException : Exception
    {
        public Type EntityTypeProperty { get; set; }

        public CrudFailedToFindException()
        {
        }

        public CrudFailedToFindException(string message)
            : base(message)
        {
        }

        public CrudFailedToFindException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CrudFailedToFindException(SerializationInfo info, StreamingContext context)
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

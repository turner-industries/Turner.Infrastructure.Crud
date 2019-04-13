using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Turner.Infrastructure.Crud.Exceptions
{
    [Serializable]
    public class InvalidQueryProviderTypeException : Exception
    {
        public Type QueryProviderType { get; set; }

        public InvalidQueryProviderTypeException()
            : this("The provided QueryProvider is not a valid provider type for this operation.")
        {
        }

        public InvalidQueryProviderTypeException(string message)
            : base(message)
        {
        }

        public InvalidQueryProviderTypeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidQueryProviderTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            QueryProviderType = (Type)info.GetValue(nameof(QueryProviderType), typeof(Type));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(QueryProviderType), QueryProviderType);

            base.GetObjectData(info, context);
        }
    }
}

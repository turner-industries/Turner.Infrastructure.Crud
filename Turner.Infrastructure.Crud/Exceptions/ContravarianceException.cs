using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Turner.Infrastructure.Crud.Exceptions
{
    public class ContravarianceException : BadConfigurationException
    {
        public Type BaseTypeProperty { get; set; }

        public Type DerivedTypeProperty { get; set; }

        public ContravarianceException(string property, Type tBase, Type tDerived)
            : base($"Invalid type for '{property}'.\n" +
                   $"'{tBase}' is not contravariant with '{tBase}'.")
        {
            ConfigurationProperty = property;
            BaseTypeProperty = tBase;
            DerivedTypeProperty = tDerived;
        }

        public ContravarianceException(string property, Type tBase, Type tDerived, Exception inner)
            : base($"Invalid type for '{property}'.\n" +
                   $"'{tBase}' is not contravariant with '{tBase}'.", inner)
        {
            ConfigurationProperty = property;
            BaseTypeProperty = tBase;
            DerivedTypeProperty = tDerived;
        }
        
        protected ContravarianceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            BaseTypeProperty = (Type)info.GetValue(nameof(BaseTypeProperty), typeof(Type));
            DerivedTypeProperty = (Type)info.GetValue(nameof(DerivedTypeProperty), typeof(Type));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(BaseTypeProperty), BaseTypeProperty);
            info.AddValue(nameof(DerivedTypeProperty), DerivedTypeProperty);

            base.GetObjectData(info, context);
        }
    }
}

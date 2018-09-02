using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Turner.Infrastructure.Crud.Errors
{
    [Serializable]
    public class BadCrudConfigurationException : Exception
    {
        public string ConfigurationProperty { get; set; }

        public BadCrudConfigurationException()
        {
        }

        public BadCrudConfigurationException(string message)
            : base(message)
        {
        }

        public BadCrudConfigurationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected BadCrudConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ConfigurationProperty = info.GetString(nameof(ConfigurationProperty));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(nameof(ConfigurationProperty), ConfigurationProperty);

            base.GetObjectData(info, context);
        }

    }
}

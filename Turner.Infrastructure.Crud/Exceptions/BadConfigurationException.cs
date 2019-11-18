using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Turner.Infrastructure.Crud.Exceptions
{
    [Serializable]
    public class BadConfigurationException : Exception
    {
        public string ConfigurationProperty { get; set; }

        public BadConfigurationException()
        {
        }

        public BadConfigurationException(string message)
            : base(message)
        {
        }

        public BadConfigurationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected BadConfigurationException(SerializationInfo info, StreamingContext context)
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

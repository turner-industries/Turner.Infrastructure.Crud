using System;
using System.Collections.Generic;
using System.Linq;

namespace Turner.Infrastructure.Crud.EntityFrameworkExtensions.Configuration
{
    public enum BulkConfigurationType { Insert, Update, Delete };

    internal class BulkRequestEntityConfigurationMap : Dictionary<Type, IBulkConfiguration>
    {
    }

    internal class BulkRequestConfigurationMap : Dictionary<Type, BulkRequestEntityConfigurationMap>
    {
    }
    
    internal static class BulkConfigurationManager
    {
        private static readonly Dictionary<BulkConfigurationType, BulkRequestConfigurationMap> BulkConfigurations;
        
        static BulkConfigurationManager()
        {
            BulkConfigurations = Enum.GetValues(typeof(BulkConfigurationType))
                .Cast<BulkConfigurationType>()
                .ToDictionary(x => x, x => new BulkRequestConfigurationMap());
        }

        public static BulkRequestConfigurationMap Configurations(BulkConfigurationType type)
            => BulkConfigurations[type];

        public static void Clear()
        {
            foreach (var configMap in BulkConfigurations.Values)
                configMap.Clear();
        }

        public static void SetConfiguration(BulkConfigurationType type, Type tRequest, Type tEntity, IBulkConfiguration config)
        {
            if (!BulkConfigurations[type].ContainsKey(tRequest))
                BulkConfigurations[type][tRequest] = new BulkRequestEntityConfigurationMap();
            
            BulkConfigurations[type][tRequest][tEntity] = config;
        }
    }
}

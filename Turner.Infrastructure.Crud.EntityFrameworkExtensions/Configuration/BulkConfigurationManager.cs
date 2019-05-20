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
        private static readonly Dictionary<BulkConfigurationType, BulkRequestConfigurationMap> _configurations;
        
        static BulkConfigurationManager()
        {
            _configurations = Enum.GetValues(typeof(BulkConfigurationType))
                .Cast<BulkConfigurationType>()
                .ToDictionary(x => x, x => new BulkRequestConfigurationMap());
        }

        public static BulkRequestConfigurationMap Configurations(BulkConfigurationType type)
            => _configurations[type];

        public static void Clear()
        {
            foreach (var configMap in _configurations.Values)
                configMap.Clear();
        }

        public static void SetConfiguration(BulkConfigurationType type, Type tRequest, Type tEntity, IBulkConfiguration config)
        {
            if (!_configurations[type].ContainsKey(tRequest))
                _configurations[type][tRequest] = new BulkRequestEntityConfigurationMap();
            
            _configurations[type][tRequest][tEntity] = config;
        }
    }
}

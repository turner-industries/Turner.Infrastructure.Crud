using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Turner.Infrastructure.Crud.Configuration
{
    public class CrudProfileManager
    {
        private readonly Dictionary<Type, ICrudEntityProfile> _entityProfiles;
        private readonly Dictionary<Type, ICrudDtoProfile> _dtoProfiles;

        public CrudProfileManager(params Assembly[] profileAssemblies)
        {
            _entityProfiles = GetImplementorsOf(profileAssemblies, typeof(CrudEntityProfile<>))
                .ToDictionary(x => x, x => (ICrudEntityProfile) Activator.CreateInstance(x));

            _dtoProfiles = GetImplementorsOf(profileAssemblies, typeof(CrudDtoProfile<>))
                .ToDictionary(x => x, x => (ICrudDtoProfile) Activator.CreateInstance(x));
        }

        private static IEnumerable<Type> GetImplementorsOf(Assembly[] assemblies, Type genericType)
        {
            return assemblies
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => !x.IsAbstract &&
                            x.BaseType != null &&
                            x.BaseType.IsGenericType &&
                            x.BaseType.GetGenericTypeDefinition() == genericType);
        }
    }
}

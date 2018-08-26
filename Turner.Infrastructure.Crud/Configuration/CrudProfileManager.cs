using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Turner.Infrastructure.Crud.Configuration
{
    public class CrudProfileManager
    {
        private Dictionary<Type, ICrudEntityProfile> _profiles;

        public CrudProfileManager(params Assembly[] profileAssemblies)
        {
            _profiles = profileAssemblies
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => !x.IsAbstract &&
                            x.BaseType != null &&
                            x.BaseType.IsGenericType &&
                            x.BaseType.GetGenericTypeDefinition() == typeof(CrudEntityProfile<>))
                .ToDictionary(x => x, x => (ICrudEntityProfile) Activator.CreateInstance(x));
        }
    }
}

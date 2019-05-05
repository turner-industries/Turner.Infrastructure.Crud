using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Requests;

namespace Turner.Infrastructure.Crud.Configuration
{
    public class CrudConfigManager
    {
        private readonly Assembly[] _profileAssemblies;
        
        private readonly Dictionary<Type, ICrudRequestConfig> _requestConfigs
            = new Dictionary<Type, ICrudRequestConfig>();

        private readonly Type[] _allProfiles;

        public CrudConfigManager(params Assembly[] profileAssemblies)
        {
            _profileAssemblies = profileAssemblies;

            _allProfiles = _profileAssemblies
                .SelectMany(x => x.GetExportedTypes())
                .Where(x =>
                    x.BaseType != null &&
                    x.BaseType.IsGenericType &&
                    (x.BaseType.GetGenericTypeDefinition() == typeof(UniversalRequestProfile<>) ||
                     x.BaseType.GetGenericTypeDefinition() == typeof(CrudRequestProfile<>) ||
                     x.BaseType.GetGenericTypeDefinition() == typeof(CrudBulkRequestProfile<,>)))
                .ToArray();
        }

        public ICrudRequestConfig GetRequestConfigFor<TRequest>() 
            => BuildRequestConfigFor(typeof(TRequest));

        public ICrudRequestConfig GetRequestConfigFor(Type tRequest) 
            => BuildRequestConfigFor(tRequest);
        
        private IEnumerable<Type> FindRequestProfilesFor(Type tRequest)
        {
            Type GetProfileRequestType(Type tProfile) => tProfile.BaseType.GenericTypeArguments[0];

            bool IsGenericallyCompatible(Type first, Type second)
            {
                var firstArgs = first.GetGenericArguments();
                var secondArgs = second.GetGenericArguments();
                
                return firstArgs.Length == secondArgs.Length &&
                    firstArgs
                        .Zip(secondArgs, (a, b) => new Tuple<Type, Type>(a, b))
                        .All(x => x.Item2.IsGenericParameter || x.Item1 == x.Item2);
            }

            Type InstantiateProfile(Type tProfile)
            {
                if (!tProfile.IsGenericTypeDefinition)
                    return tProfile;
                
                var argMap = GetProfileRequestType(tProfile)
                    .GetGenericArguments()
                    .Zip(tRequest.GetGenericArguments(), 
                        (a, b) => new Tuple<Type, Type>(a, a.IsGenericParameter ? b : null))
                    .Where(x => x.Item2 != null)
                    .ToDictionary(x => x.Item1, x => x.Item2);

                var args = tProfile.GetGenericArguments()
                    .Where(x => argMap.ContainsKey(x))
                    .Select(x => argMap[x])
                    .ToArray();

                if (args.Length != tProfile.GetGenericArguments().Length)
                {
                    throw new BadCrudConfigurationException(
                        $"Failed to determine arguments for profile '{tProfile}'.\r\n" +
                        $"Profiles may not contain more generic arguments than their requests.'");
                }

                return tProfile.MakeGenericType(args);
            }

            if (!tRequest.IsGenericType)
                return _allProfiles.Where(x => !x.IsAbstract && GetProfileRequestType(x) == tRequest);

            var requestDefinition = tRequest.GetGenericTypeDefinition();

            return _allProfiles
                .Where(x =>
                    GetProfileRequestType(x).IsGenericType &&
                    GetProfileRequestType(x).GetGenericTypeDefinition() == requestDefinition &&
                    IsGenericallyCompatible(tRequest, GetProfileRequestType(x)))
                .OrderBy(x => 
                    GetProfileRequestType(x).GetGenericArguments().Count(y => !y.IsGenericParameter))
                .Select(InstantiateProfile);
        }

        private CrudRequestProfile GetRequestProfileFor(Type tRequest)
        {
            if (!typeof(ICrudRequest).IsAssignableFrom(tRequest))
                return GetUniversalRequestProfileFor(tRequest);

            var tProfile = typeof(IBulkRequest).IsAssignableFrom(tRequest)
                ? typeof(DefaultBulkCrudRequestProfile<>).MakeGenericType(tRequest)
                : typeof(DefaultCrudRequestProfile<>).MakeGenericType(tRequest);
            
            var profile = (CrudRequestProfile)Activator.CreateInstance(tProfile);

            profile.Inherit(tRequest
                .BuildTypeHierarchyDown()
                .SelectMany(FindRequestProfilesFor)
                .Select(x => (CrudRequestProfile)Activator.CreateInstance(x)));

            return profile;
        }

        private CrudRequestProfile GetUniversalRequestProfileFor(Type tRequest)
        {
            var tProfile = typeof(DefaultUniversalRequestProfile<>).MakeGenericType(tRequest);
            var profile = (CrudRequestProfile)Activator.CreateInstance(tProfile);

            profile.Inherit(tRequest
                .BuildTypeHierarchyDown()
                .SelectMany(FindRequestProfilesFor)
                .Select(x => (CrudRequestProfile)Activator.CreateInstance(x)));

            return profile;
        }

        private ICrudRequestConfig BuildRequestConfigFor(Type tRequest)
        {
            if (_requestConfigs.TryGetValue(tRequest, out var config))
                return config;
            
            config = GetRequestProfileFor(tRequest).BuildConfiguration();

            _requestConfigs[tRequest] = config;

            return config;
        }
    }
}

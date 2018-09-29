using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Turner.Infrastructure.Crud.Exceptions;
using Turner.Infrastructure.Crud.Requests;

namespace Turner.Infrastructure.Crud.Configuration
{
    internal class CrudConfigManager
    {
        private readonly Assembly[] _profileAssemblies;
        private readonly Dictionary<Type, CrudRequestProfile> _requestProfiles
            = new Dictionary<Type, CrudRequestProfile>();
        private readonly Dictionary<Type, ICrudRequestConfig> _requestConfigs
            = new Dictionary<Type, ICrudRequestConfig>();

        public CrudConfigManager(params Assembly[] profileAssemblies)
        {
            _profileAssemblies = profileAssemblies;
            
            BuildRequestConfigurations();
        }

        public ICrudRequestConfig GetRequestConfigFor<TRequest>() 
            => BuildRequestConfigFor(typeof(TRequest));

        public ICrudRequestConfig GetRequestConfigFor(Type tRequest) 
            => BuildRequestConfigFor(tRequest);

        private void BuildRequestConfigurations()
        {
            var requests = _profileAssemblies
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => x.IsClass &&
                            !x.IsAbstract &&
                            !x.IsGenericTypeDefinition &&
                            typeof(ICrudRequest).IsAssignableFrom(x));

            foreach (var request in requests)
                BuildRequestConfigFor(request);
        }
        
        private CrudRequestProfile GetRequestProfileFor(Type tRequest)
        {
            if (_requestProfiles.TryGetValue(tRequest, out var profile))
                return profile;

            if (!typeof(ICrudRequest).IsAssignableFrom(tRequest))
                throw new BadCrudConfigurationException($"{tRequest} is not an ICrudRequest");

            var profiles = new List<CrudRequestProfile>();

            var allProfiles = _profileAssemblies
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => 
                    !x.IsAbstract &&
                    x.BaseType != null &&
                    x.BaseType.IsGenericType &&
                    x.BaseType.GetGenericTypeDefinition() == typeof(CrudRequestProfile<>))
                .ToArray();
            
            profiles.AddRange(allProfiles
                .Where(x => x.BaseType.GenericTypeArguments[0] == tRequest)
                .Select(x => (CrudRequestProfile)Activator.CreateInstance(x)));
            
            if (tRequest.IsGenericType)
            {
                var tGenericRequest = tRequest.GetGenericTypeDefinition();
                var tProfiles = allProfiles
                    .Where(x =>
                        x.IsGenericTypeDefinition &&
                        x.BaseType.GenericTypeArguments[0].IsGenericType &&
                        x.BaseType.GenericTypeArguments[0].GetGenericTypeDefinition() == tGenericRequest)
                    .Select(x => x.MakeGenericType(tRequest.GenericTypeArguments));

                profiles.AddRange(tProfiles
                    .Select(x => (CrudRequestProfile) Activator.CreateInstance(x)));
            }

            if (!profiles.Any())
            {
                profiles.Add((CrudRequestProfile) Activator.CreateInstance(
                    typeof(DefaultCrudRequestProfile<>).MakeGenericType(tRequest)));
            }
            
            profiles.AddRange(new[] { tRequest.BaseType }
                .Concat(tRequest.GetInterfaces())
                .Where(x => x != null && typeof(ICrudRequest).IsAssignableFrom(x))
                .Select(GetRequestProfileFor));

            profile = profiles.First();
            profile.Inherit(profiles.Skip(1).Where(x => x != null));

            _requestProfiles[tRequest] = profile;

            return profile;
        }

        private ICrudRequestConfig BuildRequestConfigFor(Type tRequest)
        {
            if (_requestConfigs.TryGetValue(tRequest, out var config))
                return config;

            config = (ICrudRequestConfig) Activator.CreateInstance(
                typeof(CrudRequestConfig<>).MakeGenericType(tRequest));

            GetRequestProfileFor(tRequest).Apply(config);

            _requestConfigs[tRequest] = config;

            return config;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Turner.Infrastructure.Crud.Configuration
{
    public class CrudConfigManager
    {
        private readonly Dictionary<Type, ICrudEntityProfile> _entityProfiles;
        private readonly Dictionary<Type, ICrudDtoProfile> _dtoProfiles;
        private readonly Dictionary<Type, ICrudRequestConfig> _requestConfigs;

        public CrudConfigManager(params Assembly[] profileAssemblies)
        { 
            _entityProfiles = GetProfileTypesOf<ICrudEntityProfile>(profileAssemblies, typeof(CrudEntityProfile<>));
            _dtoProfiles = GetProfileTypesOf<ICrudDtoProfile>(profileAssemblies, typeof(CrudDtoProfile<>));
            
            var requestProfiles = GetProfileTypesOf<ICrudRequestProfile>(profileAssemblies, typeof(CrudRequestProfile<>));
            _requestConfigs = requestProfiles.ToDictionary(kv => kv.Key, kv => kv.Value.BuildConfig());
        }

        public ICrudEntityProfile GetEntityProfileFor<TEntity>() where TEntity : class
            => FindProfile(typeof(TEntity), _entityProfiles)
               ?? new DefaultCrudEntityProfile<TEntity>();

        public ICrudEntityProfile GetEntityProfileFor(Type tEntity)
            => FindProfile(tEntity, _entityProfiles)
               ?? (ICrudEntityProfile) Activator.CreateInstance(typeof(DefaultCrudEntityProfile<>).MakeGenericType(tEntity));

        public ICrudDtoProfile GetDtoProfileFor<TDto>() where TDto : class
            => FindProfile(typeof(TDto), _dtoProfiles)
               ?? new DefaultCrudDtoProfile<TDto>();

        public ICrudDtoProfile GetDtoProfileFor(Type tDto)
            => FindProfile(tDto, _dtoProfiles)
               ?? (ICrudDtoProfile) Activator.CreateInstance(typeof(DefaultCrudDtoProfile<>).MakeGenericType(tDto));

        public ICrudRequestConfig GetRequestConfigFor<TRequest>() 
            => FindConfig(typeof(TRequest), _requestConfigs) 
               ?? new DefaultCrudRequestConfig<TRequest>();

        public ICrudRequestConfig GetRequestConfigFor(Type tRequest) 
            => FindConfig(tRequest, _requestConfigs) 
               ?? (ICrudRequestConfig) Activator.CreateInstance(typeof(DefaultCrudRequestConfig<>).MakeGenericType(tRequest));

        private static Dictionary<Type, TProfile> GetProfileTypesOf<TProfile>(Assembly[] assemblies, Type tOpenGeneric)
        {
            // TODO: Reverse logic.
            // Rather than searching for all profile types, search for all entity/dto/request types
            // and create a profile for it.  This will improve the performance of default profiles.

            return assemblies
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => !x.IsAbstract &&
                            !x.IsGenericTypeDefinition &&
                            x.BaseType != null &&
                            x.BaseType.IsGenericType &&
                            x.BaseType.GetGenericTypeDefinition() == tOpenGeneric)
                .ToDictionary(x => x.BaseType.GenericTypeArguments[0], x => (TProfile) Activator.CreateInstance(x));
        }

        private static TConfig FindConfig<TConfig>(Type needle, IReadOnlyDictionary<Type, TConfig> haystack)
            where TConfig : class
        {
            if (haystack.TryGetValue(needle, out var config))
                return config;

            var parents = new[] { needle.BaseType }
                .Concat(needle.GetInterfaces())
                .Where(x => x != null);

            foreach (var tParent in parents)
            {
                config = FindConfig(tParent, haystack);
                if (config != null)
                    return config;
            }

            return null;
        }

        private static TProfile FindProfile<TProfile>(Type needle, IReadOnlyDictionary<Type, TProfile> haystack)
            where TProfile : class
        {
            if (haystack.TryGetValue(needle, out var profile))
                return profile;

            var parents = new[] { needle.BaseType }
                .Concat(needle.GetInterfaces())
                .Where(x => x != null);

            foreach (var tParent in parents)
            {
                profile = FindProfile(tParent, haystack);
                if (profile != null)
                    return profile;
            }

            return null;
        }
    }
}

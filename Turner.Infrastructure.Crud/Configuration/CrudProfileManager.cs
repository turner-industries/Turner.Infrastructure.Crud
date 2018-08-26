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
        private readonly Dictionary<Type, ICrudRequestProfile> _requestProfiles;

        public CrudProfileManager(params Assembly[] profileAssemblies)
        { 
            _entityProfiles = GetProfileTypesOf<ICrudEntityProfile>(profileAssemblies, typeof(CrudEntityProfile<>));
            _dtoProfiles = GetProfileTypesOf<ICrudDtoProfile>(profileAssemblies, typeof(CrudDtoProfile<>));
            _requestProfiles = GetProfileTypesOf<ICrudRequestProfile>(profileAssemblies, typeof(CrudRequestProfile<>));
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

        public ICrudRequestProfile GetRequestProfileFor<TRequest>() 
            => FindProfile(typeof(TRequest), _requestProfiles) 
               ?? new DefaultCrudRequestProfile<TRequest>();

        public ICrudRequestProfile GetRequestProfileFor(Type tRequest) 
            => FindProfile(tRequest, _requestProfiles) 
               ?? (ICrudRequestProfile) Activator.CreateInstance(typeof(DefaultCrudRequestProfile<>).MakeGenericType(tRequest));

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

        private static TProfile FindProfile<TProfile>(Type needle, Dictionary<Type, TProfile> haystack)
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

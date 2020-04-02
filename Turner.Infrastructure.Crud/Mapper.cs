using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using SimpleInjector;

namespace Turner.Infrastructure.Crud
{
    internal static class Mapper
    {
        private static Container _container;
        private static Lazy<IMapper> _lazyMapper;

        public static void Initialize(Container container)
        {
            _container = container;

            _lazyMapper = new Lazy<IMapper>(() => _container.GetInstance<IMapper>());
        }

        public static IMapper Instance => _lazyMapper.Value;

        public static TDestination Map<TDestination>(object source) => Instance.Map<TDestination>(source);

        public static TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
            => Instance.Map<TDestination>(source, opts);

        public static TDestination Map<TSource, TDestination>(TSource source)
            => Instance.Map<TSource, TDestination>(source);

        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
            => Instance.Map(source, destination);
    }

    public static class AutoMapperExtensions
    {
        public static IQueryable<TDestination> ProjectTo<TDestination>(
            this IQueryable source,
            params Expression<Func<TDestination, object>>[] membersToExpand)
            => source.ProjectTo(Mapper.Instance.ConfigurationProvider, null, membersToExpand);

        public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source,
            object parameters,
            params Expression<Func<TDestination, object>>[] membersToExpand)
            => source.ProjectTo(Mapper.Instance.ConfigurationProvider, parameters, membersToExpand);
    }
}

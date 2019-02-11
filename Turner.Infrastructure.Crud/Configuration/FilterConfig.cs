using System.Collections.Generic;
using System.Linq;

namespace Turner.Infrastructure.Crud.Configuration
{
    public class FilterConfig
    {
        private List<IFilterFactory> _filterFactories = new List<IFilterFactory>();
        
        internal void SetFilters(List<IFilterFactory> filterFactories)
        {
            _filterFactories = filterFactories;
        }

        internal void AddFilters(IEnumerable<IFilterFactory> filterFactories)
        {
            _filterFactories.AddRange(filterFactories);
        }

        public List<IBoxedFilter> GetFilters()
            => _filterFactories.Select(x => x.Create()).ToList();
    }
}

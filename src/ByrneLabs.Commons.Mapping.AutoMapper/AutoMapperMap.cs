using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.Configuration;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Mapping.AutoMapper
{
    [PublicAPI]
    public abstract class AutoMapperMap<TFrom, TTo> : IMapper<TFrom, TTo>
    {
        private readonly object _lockSync = new object();
        private bool _initialized;
        private MapperConfiguration _mapperConfiguration;

        public abstract void CreateMap(IMapperConfigurationExpression mapperConfiguration);

        public void Map(TFrom fromSource, TTo toTarget)
        {
            Initialize();
            IMapper mapper = new Mapper(_mapperConfiguration);
            mapper.Map(fromSource, toTarget);
        }

        public virtual TTo Map(TFrom fromSource)
        {
            Initialize();
            IMapper mapper = new Mapper(_mapperConfiguration);
            return mapper.Map<TTo>(fromSource);
        }

        public virtual IEnumerable<TTo> Map(IEnumerable<TFrom> fromSource)
        {
            Initialize();
            var to = fromSource.Select(Map);
            /*
             * NOTE: If you are denormalizing an object, the same destination object will appear multiple times in the results. It is the actual instance that is duplicated, not multiple copies of the same entity. -- Jonathan Byrne 01/30/2018
             */
            var distinctTo = to.DistinctInstances().ToList();
            return distinctTo;
        }

        private void Initialize()
        {
            lock (_lockSync)
            {
                if (!_initialized)
                {
                    var configurationExpression = new MapperConfigurationExpression();
                    CreateMap(configurationExpression);
                    _mapperConfiguration = new MapperConfiguration(configurationExpression);
                    _mapperConfiguration.AssertConfigurationIsValid();
                    _initialized = true;
                }
            }
        }
    }
}

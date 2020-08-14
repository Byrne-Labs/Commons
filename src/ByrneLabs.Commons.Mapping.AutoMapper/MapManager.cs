using System;
using System.Collections.Generic;
using ByrneLabs.Commons.Ioc;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Mapping.AutoMapper
{
    [PublicAPI]
    public class MapManager : IMapManager
    {
        private readonly IContainer _container;
        private readonly object _lockSync = new object();
        private readonly IDictionary<Tuple<Type, Type>, object> _mappers = new Dictionary<Tuple<Type, Type>, object>();

        public MapManager(IContainer container)
        {
            _container = container;
        }

        public void Map<TFrom, TTo>(TFrom fromSource, TTo toTarget)
        {
            var mapper = GetMapper<TFrom, TTo>();
            mapper.Map(fromSource, toTarget);
        }

        public TTo Map<TFrom, TTo>(TFrom fromSource)
        {
            var mapper = GetMapper<TFrom, TTo>();
            return mapper.Map(fromSource);
        }

        public IEnumerable<TTo> Map<TFrom, TTo>(IEnumerable<TFrom> fromSource)
        {
            var mapper = GetMapper<TFrom, TTo>();
            return mapper.Map(fromSource);
        }

        private IMapper<TFrom, TTo> GetMapper<TFrom, TTo>()
        {
            lock (_lockSync)
            {
                var from = typeof(TFrom);
                var to = typeof(TTo);
                var key = new Tuple<Type, Type>(from, to);
                if (!_mappers.ContainsKey(key))
                {
                    var mapperType = typeof(IMapper<,>);
                    var genericMapperType = mapperType.MakeGenericType(from, to);
                    var mapper = _container.Resolve(genericMapperType);
                    if (mapper == null)
                    {
                        throw new ArgumentException("No mapper has been registered from " + from.FullName + " to " + to.FullName);
                    }

                    _mappers.Add(key, mapper);
                }

                return (IMapper<TFrom, TTo>) _mappers[key];
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Serialization
{
    [PublicAPI]
    public static class SerializerAdapterFactory
    {
        private static IList<ISerializerAdapter> _adapters;
        private static readonly object _lockSync = new object();

        public static void AddAdapter<T>()
        {
            AddAdapter(typeof(T));
        }

        public static void AddAdapter(Type adapterType)
        {
            Initialize();
            lock (_lockSync)
            {
                var adapter = (ISerializerAdapter) Activator.CreateInstance(adapterType);
                _adapters.Add(adapter);
            }
        }

        public static ISerializerAdapter GetAdapter(string contentMediaType)
        {
            Initialize();
            lock (_lockSync)
            {
                return _adapters.SingleOrDefault(adapter => adapter.SupportedContentMediaTypes.Contains(contentMediaType));
            }
        }

        [SuppressMessage("ReSharper", "LoopCanBePartlyConvertedToQuery", Justification = "Code is easier to read with more logic in the loop")]
        private static void Initialize()
        {
            lock (_lockSync)
            {
                if (_adapters == null)
                {
                    _adapters = new List<ISerializerAdapter>();
                    foreach (var adapterType in typeof(SerializerAdapterFactory).GetTypeInfo().Assembly.DefinedTypes.Where(type => type.IsSubclassOf(typeof(ISerializerAdapter))))
                    {
                        var adapter = (ISerializerAdapter) Activator.CreateInstance(adapterType.BaseType);
                        _adapters.Add(adapter);
                    }
                }
            }
        }
    }
}

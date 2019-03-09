using ByrneLabs.Commons.Ioc;
using Microsoft.Extensions.Logging;

namespace ByrneLabs.Commons.Logging
{
    public static class DefaultLoggerFactory
    {
        private static IContainer _container;

        public static ILogger<T> CreateLogger<T>() => _container.Resolve<ILogger<T>>();

        public static void Initialize(IContainer container)
        {
            _container = container;
        }
    }
}

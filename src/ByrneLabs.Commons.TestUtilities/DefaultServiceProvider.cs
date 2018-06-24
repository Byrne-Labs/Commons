using System;
using ByrneLabs.Commons.Ioc;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.TestUtilities
{
    [PublicAPI]
    public static class DefaultServiceProvider
    {
        private static readonly SimpleContainerProvider _instance = new SimpleContainerProvider(true);

        public static IServiceProvider CreateChildContainer() => new SimpleContainerProvider(_instance, false);

        public static IServiceProvider CreateEmptyContainer() => new SimpleContainerProvider(false);
    }
}

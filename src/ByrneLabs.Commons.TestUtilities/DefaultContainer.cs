using ByrneLabs.Commons.Ioc;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.TestUtilities
{
    [PublicAPI]
    public static class DefaultContainer
    {
        private static readonly SimpleContainerProvider _instance = new SimpleContainerProvider(true);

        public static IContainer CreateChildContainer() => new SimpleContainerProvider(_instance);

        public static IContainer CreateEmptyContainer() => new SimpleContainerProvider(false);
    }
}

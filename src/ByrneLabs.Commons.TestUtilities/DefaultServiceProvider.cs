using System;

namespace ByrneLabs.Commons.TestUtilities
{
    public static class DefaultServiceProvider
    {
        public static IServiceProvider CreateChildContainer() => throw new NotImplementedException();

        public static IServiceProvider CreateEmptyContainer() => throw new NotImplementedException();
    }
}

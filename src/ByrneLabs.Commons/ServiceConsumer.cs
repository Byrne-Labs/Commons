using System;
using JetBrains.Annotations;

namespace ByrneLabs.Commons
{
    [PublicAPI]
    public abstract class ServiceConsumer
    {
        protected ServiceConsumer(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        protected IServiceProvider ServiceProvider { get; }
    }
}

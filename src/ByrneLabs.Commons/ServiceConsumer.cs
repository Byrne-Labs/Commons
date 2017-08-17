using System;

namespace ByrneLabs.Commons
{
    public abstract class ServiceConsumer
    {
        protected ServiceConsumer(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        protected IServiceProvider ServiceProvider { get; }
    }
}

using System.Diagnostics.CodeAnalysis;
using ByrneLabs.Commons.Ioc;
using Microsoft.Extensions.DependencyInjection;

namespace ByrneLabs.Commons.Persistence
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Instantiated by reflection")]
    public class ContainerConfig : IContainerRegistrar
    {
        public void RegisterComponents(IContainer container)
        {
            container.RegisterType<IConnectionFactoryRegistry, DefaultConnectionFactoryRegistry>(ServiceLifetime.Singleton);
        }
    }
}

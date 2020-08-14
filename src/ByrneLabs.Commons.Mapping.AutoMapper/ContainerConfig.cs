using ByrneLabs.Commons.Ioc;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace ByrneLabs.Commons.Mapping.AutoMapper
{
    [PublicAPI]
    public class ContainerConfig : IContainerRegistrar
    {
        public void RegisterComponents(IContainer container)
        {
            container.RegisterType<IMapManager, MapManager>(ServiceLifetime.Singleton);
        }
    }
}

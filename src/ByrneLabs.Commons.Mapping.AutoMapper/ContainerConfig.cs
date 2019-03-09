using ByrneLabs.Commons.Ioc;
using Microsoft.Extensions.DependencyInjection;

namespace ByrneLabs.Commons.Mapping.AutoMapper
{
    public class ContainerConfig : IContainerRegistrar
    {
        public void RegisterComponents(IContainer container)
        {
            container.RegisterType<IMapManager, MapManager>(ServiceLifetime.Singleton);
        }
    }
}

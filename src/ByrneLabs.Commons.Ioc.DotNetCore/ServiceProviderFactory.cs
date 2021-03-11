using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace ByrneLabs.Commons.Ioc.DotNetCore
{
    [PublicAPI]
    public class ServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
    {
        public ContainerBuilder CreateBuilder(IServiceCollection services) => new ContainerBuilder { Services = services };

        public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder) => new DotNetCoreContainerProvider(true, containerBuilder.Services);
    }
}

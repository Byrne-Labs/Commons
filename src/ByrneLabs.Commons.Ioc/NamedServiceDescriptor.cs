using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace ByrneLabs.Commons.Ioc
{
    [PublicAPI]
    public class NamedServiceDescriptor : ServiceDescriptor
    {
        public NamedServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, string name) : base(serviceType, implementationType, lifetime)
        {
            Name = name;
        }

        public NamedServiceDescriptor(Type serviceType, object instance, string name) : base(serviceType, instance)
        {
            Name = name;
        }

        public NamedServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime, string name) : base(serviceType, factory, lifetime)
        {
            Name = name;
        }

        public string Name { get; }

        public static NamedServiceDescriptor ConvertFrom(ServiceDescriptor serviceDescriptor, string name = null)
        {
            NamedServiceDescriptor namedServiceDescriptor;
            if (serviceDescriptor is NamedServiceDescriptor descriptor)
            {
                namedServiceDescriptor = descriptor;
            }
            else if (serviceDescriptor.ImplementationFactory != null)
            {
                namedServiceDescriptor = new NamedServiceDescriptor(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationFactory, serviceDescriptor.Lifetime, name);
            }
            else if (serviceDescriptor.ImplementationInstance != null)
            {
                namedServiceDescriptor = new NamedServiceDescriptor(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationInstance, name);
            }
            else if (serviceDescriptor.ImplementationType != null)
            {
                namedServiceDescriptor = new NamedServiceDescriptor(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType, serviceDescriptor.Lifetime, name);
            }
            else
            {
                throw new ArgumentException("Unable to understand service descriptor", nameof(serviceDescriptor));
            }

            return namedServiceDescriptor;
        }

        public override bool Equals(object obj) => ReferenceEquals(obj, this) || obj is NamedServiceDescriptor castObj &&
            castObj.ImplementationFactory == ImplementationFactory &&
            castObj.ImplementationInstance == ImplementationInstance &&
            castObj.ImplementationType == ImplementationType &&
            castObj.Name == Name &&
            castObj.ServiceType == ServiceType;

        public override int GetHashCode() =>
            (ImplementationFactory?.GetHashCode() ?? 0) |
            (ImplementationInstance?.GetHashCode() ?? 0) |
            (ImplementationType?.GetHashCode() ?? 0) |
            (Name?.GetHashCode(StringComparison.InvariantCulture) ?? 0) |
            (ServiceType?.GetHashCode() ?? 0) |
            GetType().GetHashCode();
    }
}

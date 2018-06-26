using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace ByrneLabs.Commons.Ioc
{
#pragma warning disable CA1710 // Identifiers should have correct suffix -- This implements ICollection but it is primarily a container - Jonathan Byrne 06/26/2018
    public class SimpleContainerProvider : ContainerProvider
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        private readonly IContainer _parentContainer;
        private List<NamedServiceDescriptor> _serviceRegistry = new List<NamedServiceDescriptor>();

        public SimpleContainerProvider(bool autoRegister)
        {
            if (autoRegister)
            {
                AutoRegister();
            }
        }

        public SimpleContainerProvider(IContainer parentContainer, bool autoRegister)
        {
            _parentContainer = parentContainer;
            if (autoRegister)
            {
                AutoRegister();
            }
        }

        public override ServiceDescriptor this[int index]
        {
            get
            {
                lock (_serviceRegistry)
                {
                    return _serviceRegistry[index];
                }
            }
            set
            {
                lock (_serviceRegistry)
                {
                    _serviceRegistry[index] = NamedServiceDescriptor.ConvertFrom(value);
                }
            }
        }
        public override int Count
        {
            get
            {
                lock (_serviceRegistry)
                {
                    return _serviceRegistry.Count;
                }
            }
        }

        public override bool IsReadOnly => false;

        public override IDictionary<Type, Type> RegisteredTypes
        {
            get
            {
                lock (_serviceRegistry)
                {
                    return _serviceRegistry.Where(serviceDescriptor => serviceDescriptor.ImplementationType != null).ToDictionary(serviceDescriptor => serviceDescriptor.ServiceType, serviceDescriptor => serviceDescriptor.ImplementationType);
                }
            }
        }

        public override void Add(ServiceDescriptor item)
        {
            lock (_serviceRegistry)
            {
                _serviceRegistry.Add(NamedServiceDescriptor.ConvertFrom(item));
            }
        }

        public override bool CanResolve(Type type, string name)
        {
            lock (_serviceRegistry)
            {
                return _serviceRegistry.Any(serviceDescriptor => serviceDescriptor.ServiceType == type && serviceDescriptor.Name == name);
            }
        }

        public override void Clear()
        {
            throw new NotSupportedException();
        }

        public override bool Contains(ServiceDescriptor item)
        {
            lock (_serviceRegistry)
            {
                return _serviceRegistry.Contains(item as NamedServiceDescriptor ?? NamedServiceDescriptor.ConvertFrom(item));
            }
        }

        public override void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            lock (_serviceRegistry)
            {
                _serviceRegistry.Cast<ServiceDescriptor>().ToList().CopyTo(array, arrayIndex);
            }
        }

        public override IContainer CreateChildContainer() => throw new NotSupportedException();

        public override IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            lock (_serviceRegistry)
            {
                return _serviceRegistry.GetEnumerator();
            }
        }

        public override int IndexOf(ServiceDescriptor item)
        {
            lock (_serviceRegistry)
            {
                return _serviceRegistry.IndexOf(NamedServiceDescriptor.ConvertFrom(item));
            }
        }

        public override void Insert(int index, ServiceDescriptor item)
        {
            lock (_serviceRegistry)
            {
                _serviceRegistry.Insert(index, NamedServiceDescriptor.ConvertFrom(item));
            }
        }

        public override void RegisterInstance(Type registrationType, object instance, string name)
        {
            lock (_serviceRegistry)
            {
                _serviceRegistry.Add(new NamedServiceDescriptor(registrationType, instance, name));
            }
        }

        public override void RegisterInterceptor(Type target, Type interceptor, string name)
        {
            throw new NotSupportedException();
        }

        public override void RegisterType(Type fromType, Type toType, string name, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            lock (_serviceRegistry)
            {
                _serviceRegistry.Add(new NamedServiceDescriptor(fromType, toType, serviceLifetime, name));
            }
        }

        public override bool Remove(ServiceDescriptor item)
        {
            lock (_serviceRegistry)
            {
                return _serviceRegistry.Remove(NamedServiceDescriptor.ConvertFrom(item));
            }
        }

        public override void RemoveAt(int index)
        {
            lock (_serviceRegistry)
            {
                _serviceRegistry.RemoveAt(index);
            }
        }

        public override object Resolve(Type type, string name)
        {
            lock (_serviceRegistry)
            {
                var registeredService = _serviceRegistry.SingleOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == type && serviceDescriptor.Name == name);
                object serviceInstance;
                switch (registeredService)
                {
                    case null when _parentContainer == null:
                        throw new ArgumentException("No service descriptor found");
                    case null:
                        serviceInstance = _parentContainer.Resolve(type, name);
                        break;
                    default:
                        if (registeredService.ServiceType != null)
                        {
                            serviceInstance = Activator.CreateInstance(registeredService.ServiceType);
                        }
                        else if (registeredService.ImplementationFactory != null)
                        {
                            serviceInstance = registeredService.ImplementationFactory.Invoke(this);
                        }
                        else
                        {
                            serviceInstance = registeredService.ImplementationInstance;
                        }

                        break;
                }

                return serviceInstance;
            }
        }

        protected override void Dispose(bool disposedManaged)
        {
        }
    }
}

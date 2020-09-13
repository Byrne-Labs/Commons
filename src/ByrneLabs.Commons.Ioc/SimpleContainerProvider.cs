using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace ByrneLabs.Commons.Ioc
{
#pragma warning disable CA1710 // Identifiers should have correct suffix -- This implements ICollection but it is primarily a container - Jonathan Byrne 06/26/2018
    public class SimpleContainerProvider : ContainerProvider
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        private readonly List<NamedServiceDescriptor> _serviceRegistry = new List<NamedServiceDescriptor>();
        private readonly IDictionary<NamedServiceDescriptor, object> _singletonInstances = new Dictionary<NamedServiceDescriptor, object>();

        public SimpleContainerProvider(bool autoRegister)
        {
            RegisterInstance(typeof(IContainer), this, null);
            if (autoRegister)
            {
                AutoRegister();
            }
        }

        public SimpleContainerProvider(IContainer parentContainer)
        {
            ParentContainer = parentContainer;
            RegisterInstance(typeof(IContainer), this, null);
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

        public override IContainer ParentContainer { get; }

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
                _serviceRegistry.Cast<ServiceDescriptor>().ToArray().CopyTo(array, arrayIndex);
            }
        }

        public override IContainer CreateChildContainer() => new SimpleContainerProvider(this);

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

        public override void RegisterFactory<T>(Func<IServiceProvider, object> factory, string name, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            lock (_serviceRegistry)
            {
                if (_serviceRegistry.Any(serviceDescriptor => serviceDescriptor.Name == name && serviceDescriptor.ServiceType == typeof(T)))
                {
                    throw new ArgumentException("A service descriptor with the same service type and name has already been registered");
                }

                _serviceRegistry.Add(new NamedServiceDescriptor(typeof(T), factory, serviceLifetime, name));
            }
        }

        public sealed override void RegisterInstance(Type registrationType, object instance, string name)
        {
            lock (_serviceRegistry)
            {
                if (_serviceRegistry.Any(serviceDescriptor => serviceDescriptor.Name == name && serviceDescriptor.ServiceType == registrationType))
                {
                    throw new ArgumentException("A service descriptor with the same service type and name has already been registered");
                }

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
                if (_serviceRegistry.Any(serviceDescriptor => serviceDescriptor.Name == name && serviceDescriptor.ServiceType == fromType))
                {
                    throw new ArgumentException("A service descriptor with the same service type and name has already been registered");
                }

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
                var registeredServiceDescriptor = _serviceRegistry.SingleOrDefault(serviceDescriptor => serviceDescriptor.ServiceType == type && serviceDescriptor.Name == name);
                object serviceInstance;
                switch (registeredServiceDescriptor)
                {
                    case null when ParentContainer == null:
                        throw new ArgumentException("No service descriptor found");
                    case null:
                        serviceInstance = ParentContainer.Resolve(type, name);
                        break;
                    default:
                        if (_singletonInstances.ContainsKey(registeredServiceDescriptor))
                        {
                            serviceInstance = _singletonInstances[registeredServiceDescriptor];
                        }
                        else
                        {
                            if (registeredServiceDescriptor.ImplementationInstance != null)
                            {
                                serviceInstance = registeredServiceDescriptor.ImplementationInstance;
                            }
                            else if (registeredServiceDescriptor.ImplementationType != null)
                            {
                                var constructors = registeredServiceDescriptor.ImplementationType.GetConstructors();
                                if (constructors.Length > 1)
                                {
                                    throw new InvalidOperationException("Cannot create an object that has more than one constructor");
                                }

                                if (constructors.Length == 0)
                                {
                                    serviceInstance = Activator.CreateInstance(registeredServiceDescriptor.ImplementationType);
                                }
                                else
                                {
                                    var parameters = new ArrayList();
                                    foreach (var parameter in constructors.First().GetParameters())
                                    {
                                        if (!CanResolve(parameter.ParameterType))
                                        {
                                            throw new InvalidOperationException($"Cannot create an object for parameter of type {parameter.ParameterType.FullName}");
                                        }

                                        parameters.Add(Resolve(parameter.ParameterType));
                                    }

                                    serviceInstance = Activator.CreateInstance(registeredServiceDescriptor.ImplementationType, parameters.ToArray());
                                }
                            }
                            else if (registeredServiceDescriptor.ImplementationFactory != null)
                            {
                                serviceInstance = registeredServiceDescriptor.ImplementationFactory.Invoke(this);
                            }
                            else
                            {
                                throw new InvalidOperationException("Insufficient information to return instance");
                            }

                            if (registeredServiceDescriptor.Lifetime == ServiceLifetime.Scoped || registeredServiceDescriptor.Lifetime == ServiceLifetime.Singleton)
                            {
                                _singletonInstances.Add(registeredServiceDescriptor, serviceInstance);
                            }
                        }

                        break;
                }

                return serviceInstance;
            }
        }

        protected override void Dispose(bool disposedManaged)
        {
            foreach (var instance in _singletonInstances.Values.OfType<IDisposable>().Where(instance => !ReferenceEquals(instance, this)))
            {
                instance.Dispose();
            }

            lock (_serviceRegistry)
            {
                foreach (var instance in _serviceRegistry.Select(serviceDescriptor => serviceDescriptor.ImplementationInstance).OfType<IDisposable>().Where(instance => !ReferenceEquals(instance, this)))
                {
                    instance.Dispose();
                }
            }
        }
    }
}

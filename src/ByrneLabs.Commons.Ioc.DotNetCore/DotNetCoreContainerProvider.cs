using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ByrneLabs.Commons.Ioc.DotNetCore
{
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This class is technically a collection but only secondarily to being a container.")]
    [PublicAPI]
    public class DotNetCoreContainerProvider : ContainerProvider
    {
        private readonly List<string> _dirtyServiceProviders = new List<string>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _nameLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
        private readonly ConcurrentDictionary<string, IServiceProvider> _serviceProviders;
        private readonly ConcurrentDictionary<string, IServiceCollection> _services;
        private readonly ConcurrentDictionary<string, IServiceScope> _serviceScopes;
        private bool _disposing;

        public DotNetCoreContainerProvider(bool autoRegister)
        {
            var defaultServiceCollection = new ServiceCollection();
            _services = new ConcurrentDictionary<string, IServiceCollection>(new Dictionary<string, IServiceCollection> { { string.Empty, defaultServiceCollection } });
            _serviceProviders = new ConcurrentDictionary<string, IServiceProvider>(new Dictionary<string, IServiceProvider> { { string.Empty, defaultServiceCollection.BuildServiceProvider() } });
            if (autoRegister)
            {
                AutoRegister();
            }
        }

        public DotNetCoreContainerProvider(bool autoRegister, IServiceCollection services, IServiceProvider serviceProvider)
        {
            _services = new ConcurrentDictionary<string, IServiceCollection>(new Dictionary<string, IServiceCollection> { { string.Empty, services } });
            _serviceProviders = new ConcurrentDictionary<string, IServiceProvider>(new Dictionary<string, IServiceProvider> { { string.Empty, serviceProvider } });
            _services[string.Empty].AddSingleton<IContainer>(this);
            if (autoRegister)
            {
                AutoRegister();
            }
        }

        private DotNetCoreContainerProvider(ConcurrentDictionary<string, IServiceCollection> services, ConcurrentDictionary<string, IServiceScope> serviceScopes, IContainer parent)
        {
            _services = services;
            _serviceScopes = serviceScopes;
            ParentContainer = parent;
        }

        public override ServiceDescriptor this[int index]
        {
            get
            {
                LockName(null);
                var serviceDescriptor = GetNamedServiceCollection(null)[index];
                UnlockName(null);
                return serviceDescriptor;
            }
            set
            {
                LockName(null);
                GetNamedServiceCollection(null)[index] = value;
                UnlockName(null);
            }
        }

        public override int Count => GetNamedServiceCollection(null, false).Count;

        public override bool IsReadOnly => false;

        public override IContainer ParentContainer { get; }

        public override IDictionary<Type, Type> RegisteredTypes => _services[string.Empty].Where(service => !ReferenceEquals(service.ImplementationInstance, this)).ToDictionary(service => service.ServiceType, service => service.ImplementationType);

        public override void Add(ServiceDescriptor item)
        {
            LockName(null);
            GetNamedServiceCollection(null).Add(item);
            UnlockName(null);
        }

        public override bool CanResolve(Type type, string name)
        {
            LockName(null);
            var canResolve = GetNamedServiceCollection(name).Any(service => service.ServiceType == type);
            UnlockName(null);

            return canResolve;
        }

        public override void Clear()
        {
            LockName(null);
            GetNamedServiceCollection(null).Clear();
            UnlockName(null);
        }

        public override bool Contains(ServiceDescriptor item)
        {
            LockName(null);
            var contains = GetNamedServiceCollection(null, false).Contains(item);
            UnlockName(null);

            return contains;
        }

        public override void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            LockName(null);
            GetNamedServiceCollection(null, false).CopyTo(array, arrayIndex);
            UnlockName(null);
        }

        public override IContainer CreateChildContainer()
        {
            var childServiceProviders = new ConcurrentDictionary<string, IServiceScope>(_serviceProviders.Select(pair => new KeyValuePair<string, IServiceScope>(pair.Key, pair.Value.CreateScope())));
            var servicesCopy = new ConcurrentDictionary<string, IServiceCollection>(_services.Select(pair => new KeyValuePair<string, IServiceCollection>(pair.Key, new ServiceCollection { pair.Value })));
            return new DotNetCoreContainerProvider(servicesCopy, childServiceProviders, this);
        }

        public override IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            LockName(null);
            var enumerator = GetNamedServiceCollection(null, false).GetEnumerator();
            UnlockName(null);

            return enumerator;
        }

        public override int IndexOf(ServiceDescriptor item)
        {
            LockName(null);
            var index = GetNamedServiceCollection(null, false).IndexOf(item);
            UnlockName(null);

            return index;
        }

        public override void Insert(int index, ServiceDescriptor item)
        {
            LockName(null);
            GetNamedServiceCollection(null).Insert(index, item);
            UnlockName(null);
        }

        public override void RegisterFactory<T>(Func<IServiceProvider, object> factory, string name, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            LockName(name);
            var services = GetNamedServiceCollection(name);
            services.Add(new NamedServiceDescriptor(typeof(T), factory, serviceLifetime, name));
            UnlockName(name);
        }

        public override void RegisterInstance(Type registrationType, object instance, string name)
        {
            LockName(name);
            var services = GetNamedServiceCollection(name);
            services.AddSingleton(registrationType, instance);
            UnlockName(name);
        }

        public override void RegisterInterceptor(Type target, Type interceptor, string name) => throw new NotSupportedException("The ASP.NET Core dependency injection framework does not support method interception");

        public override void RegisterType(Type fromType, Type toType, string name, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            LockName(name);
            var services = GetNamedServiceCollection(name);
            switch (serviceLifetime)
            {
                case ServiceLifetime.Transient:
                    services.AddTransient(fromType, toType);
                    break;
                case ServiceLifetime.Singleton:
                    services.AddSingleton(fromType, toType);
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped(fromType, toType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
            }

            UnlockName(name);
        }

        public override bool Remove(ServiceDescriptor item)
        {
            LockName(null);
            var successful = GetNamedServiceCollection(null).Remove(item);
            UnlockName(null);

            return successful;
        }

        public override void RemoveAt(int index)
        {
            LockName(null);
            GetNamedServiceCollection(null).RemoveAt(index);
            UnlockName(null);
        }

        public override object Resolve(Type type, string name)
        {
            LockName(name);
            var resolvedObject = GetServiceProvider(name).GetService(type);
            UnlockName(name);
            return resolvedObject;
        }

        protected override void Dispose(bool disposedManaged)
        {
            if (disposedManaged && !_disposing)
            {
                _disposing = true;
                if (_serviceScopes == null)
                {
                    foreach (var serviceScope in _serviceScopes.Values)
                    {
                        serviceScope.Dispose();
                    }
                }
                else
                {
                    foreach (var serviceProvider in _serviceProviders.Values.OfType<IDisposable>())
                    {
                        serviceProvider.Dispose();
                    }
                }
            }
        }

        private IServiceCollection GetNamedServiceCollection(string name, bool markDirty = true)
        {
            IServiceCollection serviceCollection;
            if (markDirty)
            {
                lock (_dirtyServiceProviders)
                {
                    serviceCollection = _services.AddOrUpdate(name ?? string.Empty, nameNotFound => new ServiceCollection(), (nameFound, foundServiceCollection) =>
                    {
                        _dirtyServiceProviders.Add(name ?? string.Empty);
                        return foundServiceCollection;
                    });
                }
            }
            else
            {
                serviceCollection = _services.AddOrUpdate(name ?? string.Empty, nameNotFound => new ServiceCollection(), (nameFound, foundServiceCollection) => foundServiceCollection);
            }

            return serviceCollection;
        }

        private IServiceProvider GetServiceProvider(string name)
        {
            IServiceProvider serviceProvider;
            lock (_dirtyServiceProviders)
            {
                var serviceProviderDirty = _dirtyServiceProviders.Remove(name ?? string.Empty);
                if (_serviceScopes == null)
                {
                    if (serviceProviderDirty)
                    {
                        serviceProvider = _serviceProviders.AddOrUpdate(name ?? string.Empty, nameNotFound => GetNamedServiceCollection(nameNotFound).BuildServiceProvider(), (nameFound, dirtyServiceProvider) => GetNamedServiceCollection(nameFound, false).BuildServiceProvider());
                    }
                    else
                    {
                        serviceProvider = _serviceProviders.GetOrAdd(name ?? string.Empty, nameNotFound => GetNamedServiceCollection(nameNotFound).BuildServiceProvider());
                    }
                }
                else
                {
                    if (serviceProviderDirty)
                    {
                        serviceProvider = _serviceScopes.AddOrUpdate(name ?? string.Empty, nameNotFound => GetNamedServiceCollection(nameNotFound).BuildServiceProvider().CreateScope(), (nameFound, dirtyServiceProvider) => GetNamedServiceCollection(nameFound, false).BuildServiceProvider().CreateScope()).ServiceProvider;
                    }
                    else
                    {
                        serviceProvider = _serviceScopes.GetOrAdd(name ?? string.Empty, nameNotFound => GetNamedServiceCollection(nameNotFound).BuildServiceProvider().CreateScope()).ServiceProvider;
                    }
                }
            }

            return serviceProvider;
        }

        private void LockName(string name)
        {
            var semaphore = _nameLocks.GetOrAdd(name ?? string.Empty, nameNotFound => new SemaphoreSlim(1, 1));
            semaphore.Wait();
        }

        private void UnlockName(string name)
        {
            var semaphore = _nameLocks.GetOrAdd(name ?? string.Empty, nameNotFound => throw new InvalidOperationException($"The name {name ?? string.Empty} has no semaphore created to unlock"));
            semaphore.Release();
        }
    }
}

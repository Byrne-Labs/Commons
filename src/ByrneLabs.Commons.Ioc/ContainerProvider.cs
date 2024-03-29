﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace ByrneLabs.Commons.Ioc
{
    public abstract class ContainerProvider : IContainer
    {
        private static readonly string[] _ignoredAutoRegistryAssemblies = { "mscorlib", "system.", "microsoft." };
        private readonly object _lockSync = new object();
        private bool _initialized;

        public abstract ServiceDescriptor this[int index] { get; set; }

        public abstract int Count { get; }

        public abstract bool IsReadOnly { get; }

        public abstract IContainer ParentContainer { get; }

        public abstract IDictionary<Type, Type> RegisteredTypes { get; }

        private static IEnumerable<IContainerRegistrar> GetRegistrars(Assembly assembly) => assembly.GetTypes().Where(type => !type.IsInterface && typeof(IContainerRegistrar).IsAssignableFrom(type)).Select(type => (IContainerRegistrar) Activator.CreateInstance(type)).ToArray();

        private static bool IsAssemblyNotIgnored(Assembly assembly) => !_ignoredAutoRegistryAssemblies.Any(ignoredAssembly => assembly.GetName().Name.StartsWith(ignoredAssembly, true, CultureInfo.InvariantCulture));

        public abstract void Add(ServiceDescriptor item);

        public abstract bool CanResolve(Type type, string name);

        public abstract void Clear();

        public abstract bool Contains(ServiceDescriptor item);

        public abstract void CopyTo(ServiceDescriptor[] array, int arrayIndex);

        public abstract IContainer CreateChildContainer();

        public abstract IEnumerator<ServiceDescriptor> GetEnumerator();

        public abstract int IndexOf(ServiceDescriptor item);

        public abstract void Insert(int index, ServiceDescriptor item);

        public abstract void RegisterFactory<T>(Func<IServiceProvider, object> factory, string name, ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

        public abstract void RegisterInstance(Type registrationType, object instance, string name);

        public abstract void RegisterInterceptor(Type target, Type interceptor, string name);

        public abstract void RegisterType(Type fromType, Type toType, string name, ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

        public abstract bool Remove(ServiceDescriptor item);

        public abstract void RemoveAt(int index);

        public abstract object Resolve(Type type, string name);

        public bool CanResolve<T>() => CanResolve(typeof(T));

        public bool CanResolve<T>(string name) => CanResolve(typeof(T), name);

        public bool CanResolve(Type type) => CanResolve(type, null);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public object GetService(Type serviceType) => Resolve(serviceType);

        public void Register<T>() where T : IContainerRegistrar
        {
            var registrar = Activator.CreateInstance<T>();
            registrar.RegisterComponents(this);
        }

        public void RegisterFactory<T>(Func<IServiceProvider, object> factory, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) => RegisterFactory<T>(factory, null, serviceLifetime);

        public virtual void RegisterInstance<T>(T instance) => RegisterInstance(typeof(T), instance, null);

        public virtual void RegisterInstance<T>(T instance, string name) => RegisterInstance(typeof(T), instance, name);

        public virtual void RegisterInterceptor<TTarget, TInterceptor>() => RegisterInterceptor(typeof(TTarget), typeof(TInterceptor), null);

        public virtual void RegisterInterceptor<TTarget, TInterceptor>(string name) => RegisterInterceptor(typeof(TTarget), typeof(TInterceptor), name);

        public virtual void RegisterInterceptor<TInterceptor>(Type target, string name) => RegisterInterceptor(target, typeof(TInterceptor), name);

        public virtual void RegisterInterceptor<TInterceptor>(Type target) => RegisterInterceptor(target, typeof(TInterceptor), null);

        public virtual void RegisterInterceptor(Type target, Type interceptor) => RegisterInterceptor(target, interceptor, null);

        public virtual void RegisterType<TFrom, TTo>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where TTo : TFrom => RegisterType(typeof(TFrom), typeof(TTo), serviceLifetime);

        public virtual void RegisterType<TFrom, TTo>(string name, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where TTo : TFrom => RegisterType(typeof(TFrom), typeof(TTo), name, serviceLifetime);

        public virtual void RegisterType(Type fromType, Type toType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) => RegisterType(fromType, toType, null, serviceLifetime);

        public virtual T Resolve<T>(string name) => (T) Resolve(typeof(T), name);

        public virtual object Resolve(Type type) => Resolve(type, null);

        public virtual T Resolve<T>() => (T) Resolve(typeof(T), null);

        protected abstract void Dispose(bool disposedManaged);

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void AutoRegister()
        {
            lock (_lockSync)
            {
                if (!_initialized)
                {
                    var nonSystemAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(IsAssemblyNotIgnored).ToArray();
                    var registrars = nonSystemAssemblies.SelectMany(GetRegistrars).ToArray();
                    Register(registrars);
                    AppDomain.CurrentDomain.AssemblyLoad += OnCurrentDomainOnAssemblyLoad;
                    _initialized = true;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void OnCurrentDomainOnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (IsAssemblyNotIgnored(args.LoadedAssembly))
            {
                Register(GetRegistrars(args.LoadedAssembly));
            }
        }

        private void Register(IEnumerable<IContainerRegistrar> registrars)
        {
            foreach (var registrar in registrars)
            {
                registrar.RegisterComponents(this);
            }
        }
    }
}

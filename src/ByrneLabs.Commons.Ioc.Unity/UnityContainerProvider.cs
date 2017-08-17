using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace ByrneLabs.Commons.Ioc.Unity
{
    public class UnityContainerProvider : BaseContainerProvider
    {
        private bool _disposing;
        private bool _interceptionRegistered;

        public UnityContainerProvider(bool autoRegister) : this(autoRegister, new UnityContainer())
        {
        }

        private UnityContainerProvider(bool autoRegister, IUnityContainer unityContainer)
        {
            UnityContainer = unityContainer;
            UnityContainer.RegisterInstance(typeof(IContainer), this);
            if (autoRegister)
            {
                AutoRegister();
            }
        }

        public override IDictionary<Type, Type> RegisteredTypes =>
            UnityContainer.Registrations.Where(registration => !registration.RegisteredType.Namespace.StartsWith(typeof(IUnityContainer).Namespace, StringComparison.Ordinal) && !typeof(ICallHandler).IsAssignableFrom(registration.RegisteredType))
                .ToDictionary(registration => registration.RegisteredType, registration => registration.MappedToType);

        public IUnityContainer UnityContainer { get; private set; }

        private static LifetimeManager CreateLifetimeManager(ObjectLifetime objectLifetime)
        {
            LifetimeManager lifetimeManager;
            switch (objectLifetime)
            {
                case ObjectLifetime.PerContainer:
                    lifetimeManager = new ContainerControlledLifetimeManager();
                    break;
                case ObjectLifetime.Transient:
                    lifetimeManager = new TransientLifetimeManager();
                    break;
                default:
                    throw new NotSupportedException(string.Format("{0} does not support the object lifetime {1}", typeof(UnityContainerProvider).FullName, objectLifetime));
            }

            return lifetimeManager;
        }

        public override bool CanResolve(Type type, string name) => UnityContainer.IsRegistered(type, name);

        public override IContainer CreateChildContainer() => new UnityContainerProvider(false, UnityContainer.CreateChildContainer());

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override void RegisterInstance(Type registrationType, object instance, string name)
        {
            UnityContainer.RegisterInstance(registrationType, name, instance);
            OnInstanceRegistered(new ContainerInstanceRegistrationEventArgs(this, instance, name));
        }

        public override void RegisterInterceptor(Type target, Type interceptor, string name)
        {
            if (!typeof(IInterceptionBehavior).IsAssignableFrom(interceptor))
            {
                throw new ArgumentException("Generic argument TInterceptor must inherit Microsoft.Practices.Unity.InterceptionExtension.IInterceptionBehavior");
            }

            if (!_interceptionRegistered)
            {
                UnityContainer.AddNewExtension<Interception>();
                _interceptionRegistered = true;
            }
            UnityContainer.RegisterType(target, name, new Interceptor<InterfaceInterceptor>(), new InterceptionBehavior(interceptor));
        }

        public override void RegisterType(Type from, Type to, string name, ObjectLifetime objectLifetime = ObjectLifetime.Transient)
        {
            UnityContainer.RegisterType(from, to, name, CreateLifetimeManager(objectLifetime));
            OnTypeRegistered(new ContainerTypeRegistrationEventArgs(this, from, to, name));
        }

        public override object Resolve(Type type, string name) => UnityContainer.Resolve(type, name);

        protected virtual void Dispose(bool disposedManaged)
        {
            if (disposedManaged && !_disposing)
            {
                _disposing = true;
                UnityContainer.RemoveAllExtensions();
                UnityContainer.Dispose();
            }
        }
    }
}

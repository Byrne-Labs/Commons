using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace ByrneLabs.Commons.Ioc
{
    [PublicAPI]
    public interface IContainer : IServiceCollection, IServiceProvider, IDisposable
    {
        IContainer ParentContainer { get; }

        IDictionary<Type, Type> RegisteredTypes { get; }

        bool CanResolve<T>();

        bool CanResolve<T>(string name);

        bool CanResolve(Type type);

        bool CanResolve(Type type, string name);

        IContainer CreateChildContainer();

        void Register<T>() where T : IContainerRegistrar;

        void RegisterFactory<T>(Func<IServiceProvider, object> factory, ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

        void RegisterFactory<T>(Func<IServiceProvider, object> factory, string name, ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

        void RegisterInstance<T>(T instance);

        void RegisterInstance<T>(T instance, string name);

        void RegisterInstance(Type registrationType, object instance, string name);

        void RegisterInterceptor<TTarget, TInterceptor>(string name);

        void RegisterInterceptor<TInterceptor>(Type target, string name);

        void RegisterInterceptor(Type target, Type interceptor, string name);

        void RegisterInterceptor<TTarget, TInterceptor>();

        void RegisterInterceptor<TInterceptor>(Type target);

        void RegisterInterceptor(Type target, Type interceptor);

        void RegisterType<TFrom, TTo>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where TTo : TFrom;

        void RegisterType<TFrom, TTo>(string name, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where TTo : TFrom;

        void RegisterType(Type fromType, Type toType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

        void RegisterType(Type fromType, Type toType, string name, ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

        T Resolve<T>(string name);

        T Resolve<T>();

        object Resolve(Type type);

        object Resolve(Type type, string name);
    }
}

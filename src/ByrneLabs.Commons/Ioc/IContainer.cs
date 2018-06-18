using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace ByrneLabs.Commons.Ioc
{
    [PublicAPI]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This class is technically a collection but only secondarily to being a container.")]
    public interface IContainer : IServiceCollection, IServiceProvider, IDisposable
    {
        IDictionary<Type, Type> RegisteredTypes { get; }

        bool CanResolve<T>();

        bool CanResolve<T>(string name);

        bool CanResolve(Type type);

        bool CanResolve(Type type, string name);

        IContainer CreateChildContainer();

        void Register<T>() where T : IContainerRegistrar;

        void RegisterInstance<T>(T instance);

        void RegisterInstance<T>(T instance, string name);

        void RegisterInstance(Type registrationType, object instance, string name);

        void RegisterInterceptor<TTarget, TInterceptor>(string name);

        void RegisterInterceptor<TInterceptor>(Type target, string name);

        void RegisterInterceptor(Type target, Type interceptor, string name);

        void RegisterInterceptor<TTarget, TInterceptor>();

        void RegisterInterceptor<TInterceptor>(Type target);

        void RegisterInterceptor(Type target, Type interceptor);

        void RegisterType<TFrom, TTo>(ObjectLifetime objectLifetime = ObjectLifetime.Transient) where TTo : TFrom;

        void RegisterType<TFrom, TTo>(string name, ObjectLifetime objectLifetime = ObjectLifetime.Transient) where TTo : TFrom;

        void RegisterType(Type fromType, Type toType, ObjectLifetime objectLifetime = ObjectLifetime.Transient);

        void RegisterType(Type fromType, Type toType, string name, ObjectLifetime objectLifetime = ObjectLifetime.Transient);

        T Resolve<T>(string name);

        T Resolve<T>();

        object Resolve(Type type);

        object Resolve(Type type, string name);
    }
}

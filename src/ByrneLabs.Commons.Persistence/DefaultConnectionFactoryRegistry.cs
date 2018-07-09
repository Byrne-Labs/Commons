using System;
using System.Data;
using ByrneLabs.Commons.Ioc;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Persistence
{
    [PublicAPI]
    public class DefaultConnectionFactoryRegistry : IConnectionFactoryRegistry
    {
        private const string _containerRegistrationPrefix = "CONNECTION FACTORY : ";
        private readonly IContainer _container;

        public DefaultConnectionFactoryRegistry(IContainer container)
        {
            _container = container;
        }

        public IDbConnection GetConnection(string name)
        {
            if (!_container.CanResolve<IDbConnection>(_containerRegistrationPrefix + name))
            {
                throw new ArgumentException($"A connection factory named '{name}' has not been registered");
            }

            return _container.Resolve<IDbConnection>(_containerRegistrationPrefix + name);
        }

        public void RegisterFactory(string name, Func<IDbConnection> factoryMethod)
        {
            _container.RegisterFactory<IDbConnection>(provider => factoryMethod.Invoke(), _containerRegistrationPrefix + name);
        }
    }
}

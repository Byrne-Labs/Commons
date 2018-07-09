using System;
using System.Data;

namespace ByrneLabs.Commons.Persistence
{
    public interface IConnectionFactoryRegistry
    {
        IDbConnection GetConnection(string name);
        void RegisterFactory(string name, Func<IDbConnection> factoryMethod);
    }
}
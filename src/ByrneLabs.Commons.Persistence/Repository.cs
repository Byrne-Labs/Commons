using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ByrneLabs.Commons.Domain;
using ByrneLabs.Commons.Ioc;
using Dapper;
using Dapper.Contrib.Extensions;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Persistence
{
    [PublicAPI]
    public abstract class Repository<TInterface, TImplementation> : IRepository<TInterface> where TImplementation : Entity, TInterface
    {
        [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "We want this to be static to each generic type")]
        private static string _defaultBulkInsertCommand;
        [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "We want this to be static to each generic type")]
        private static string _defaultBulkUpdateCommand;
        [SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "We want this to be static to each generic type")]
        private static string _defaultSelectCommand;
        private readonly string _connectionFactoryName;
        private readonly IContainer _container;

        protected Repository(string connectionFactoryName, IContainer container)
        {
            _connectionFactoryName = connectionFactoryName;
            _container = container;
        }

        protected virtual string BulkInsertCommand
        {
            get
            {
                lock (this)
                {
                    if (_defaultBulkInsertCommand == null)
                    {
                        var properties = new List<string> { KeyColumnName };
                        properties.AddRange(typeof(TImplementation).GetProperties().Where(property => property.CanRead && property.CanWrite && !string.Equals(property.Name, nameof(Entity.EntityId), StringComparison.Ordinal)).Select(property => property.Name));
                        var columns = string.Join(", ", properties);
                        var parameters = string.Join(", ", properties.Select(property => "@" + property));
                        _defaultBulkInsertCommand = $"INSERT {typeof(TImplementation).Name} ({columns}) VALUES ({parameters})";
                    }

                    return _defaultBulkInsertCommand;
                }
            }
        }
        protected virtual string BulkUpdateCommand
        {
            get
            {
                lock (this)
                {
                    if (_defaultBulkUpdateCommand == null)
                    {
                        var properties = typeof(TImplementation).GetProperties().Where(property => property.CanRead && property.CanWrite && !string.Equals(property.Name, nameof(Entity.EntityId), StringComparison.Ordinal)).Select(property => property.Name + " = " + "@" + property.Name);
                        _defaultBulkUpdateCommand = $"UPDATE {typeof(TImplementation).Name} SET {string.Join(", ", properties)} WHERE {KeyColumnName} = @EntityId";
                    }

                    return _defaultBulkUpdateCommand;
                }
            }
        }

        protected virtual string KeyColumnName => typeof(TImplementation).Name + "Id";

        protected virtual string SelectCommand => _defaultSelectCommand ?? (_defaultSelectCommand = $"SELECT * FROM {typeof(TImplementation).Name}");

        public virtual void Delete(IEnumerable<TInterface> items)
        {
            var entities = items.Cast<TImplementation>().ToList();
            using (var connection = CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var failedDeletes = entities.Where(entity => !connection.Delete(entity, transaction));
                    transaction.Commit();
                    throw new PersistenceException("Some entities were not deleted", failedDeletes);
                }
            }
        }

        public virtual IEnumerable<TInterface> Find(object criteria)
        {
            using (var connection = CreateConnection())
            {
                var criteriaFields = criteria.GetType().GetFields().Select(field => $"{field.Name} = @{field.Name}").Union(criteria.GetType().GetProperties().Where(property => property.CanRead).Select(property => $"{property.Name} = @{property.Name}")).ToList();
                var command = $"{SelectCommand} WHERE {string.Join(" AND ", criteriaFields)}";

                return connection.Query<TImplementation>(command);
            }
        }

        public virtual IEnumerable<TInterface> FindAll()
        {
            using (var connection = CreateConnection())
            {
                return connection.GetAll<TImplementation>().Cast<TInterface>().ToList();
            }
        }

        public void Save(IEnumerable<TInterface> items)
        {
            var entities = items.Cast<TImplementation>().ToList();
            using (var connection = CreateConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    connection.Execute(BulkInsertCommand, entities.Where(item => item.NeverPersisted), transaction);
                    connection.Execute(BulkUpdateCommand, entities.Where(item => !item.NeverPersisted && item.HasChanged), transaction);
                    transaction.Commit();
                }
            }
        }

        protected IDbConnection CreateConnection() => _container.Resolve<IConnectionFactoryRegistry>().GetConnection(_connectionFactoryName);
    }
}

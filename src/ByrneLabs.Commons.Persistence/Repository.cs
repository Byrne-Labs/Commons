using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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
        private readonly string[] _defaultIgnoredEntityProperties = { nameof(Entity.EntityId), nameof(Entity.NeverPersisted), nameof(Entity.HasChanged) };

        protected Repository(string connectionFactoryName, IContainer container)
        {
            _connectionFactoryName = connectionFactoryName;
            _container = container;

            var map = new CustomPropertyTypeMap(typeof(TImplementation), (type, columnName) =>
            {
                PropertyInfo property;
                if (ColumnToPropertyMap.ContainsKey(columnName))
                {
                    property = typeof(TImplementation).GetProperty(ColumnToPropertyMap[columnName]);
                }
                else if (string.Equals(columnName, KeyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    property = typeof(TImplementation).GetProperty(nameof(Entity.EntityId));
                }
                else
                {
                    property = typeof(TImplementation).GetProperty(columnName);
                }

                return property;
            });
            SqlMapper.SetTypeMap(typeof(TImplementation), map);
        }

        protected virtual string BulkInsertCommand
        {
            get
            {
                lock (this)
                {
                    if (_defaultBulkInsertCommand == null)
                    {
                        var properties = GetPersistedPropertyNames();
                        var columns = string.Join(", ", properties);
                        var parameters = string.Join(", ", properties.Select(property => "@" + property));
                        _defaultBulkInsertCommand = $"INSERT {TableName} ({KeyColumnName}, {columns}) VALUES (@EntityId, {parameters})";
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
                        var properties = GetPersistedPropertyNames().Select(property => property + " = " + "@" + property);
                        _defaultBulkUpdateCommand = $"UPDATE {TableName} SET {string.Join(", ", properties)} WHERE {KeyColumnName} = @EntityId";
                    }

                    return _defaultBulkUpdateCommand;
                }
            }
        }

        protected virtual IDictionary<string, string> ColumnToPropertyMap { get; } = new Dictionary<string, string>();

        protected virtual IEnumerable<string> IgnoredEntityProperties { get; } = Enumerable.Empty<string>();

        protected virtual string KeyColumnName => typeof(TImplementation).Name + "Id";

        protected virtual string SelectCommand => _defaultSelectCommand ?? (_defaultSelectCommand = $"SELECT * FROM {TableName}");

        protected virtual string TableName => typeof(TImplementation).Name;

        private static IEnumerable<TImplementation> SetAsPersisted(IEnumerable<TImplementation> entities)
        {
            foreach (var entity in entities)
            {
                entity.NeverPersisted = false;
            }

            return entities;
        }

        public virtual void Delete(IEnumerable<TInterface> items)
        {
            var entities = items.Cast<TImplementation>().ToList();
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var failedDeletes = entities.Where(entity => !connection.Delete(entity, transaction));
                    transaction.Commit();
                    throw new PersistenceException("Some entities were not deleted", failedDeletes);
                }
            }
        }

        public virtual TInterface Find(Guid entityId) => Find(new[] { entityId }).SingleOrDefault();

        public virtual IEnumerable<TInterface> Find(IEnumerable<Guid> entityIds)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var command = $"{SelectCommand} WHERE {KeyColumnName} IN @EntityIds";

                return SetAsPersisted(connection.Query<TImplementation>(command, new { EntityIds = entityIds.ToArray() }).ToList());
            }
        }

        public virtual IEnumerable<TInterface> Find(object criteria)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var criteriaFields = criteria.GetType().GetFields().Select(field => $"{field.Name} = @{field.Name}").Union(criteria.GetType().GetProperties().Where(property => property.CanRead).Select(property => $"{property.Name} = @{property.Name}")).ToList();
                var command = $"{SelectCommand} WHERE {string.Join(" AND ", criteriaFields)}";

                return SetAsPersisted(connection.Query<TImplementation>(command).ToList());
            }
        }

        public virtual IEnumerable<TInterface> FindAll()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                return SetAsPersisted(connection.Query<TImplementation>(SelectCommand).ToList());
            }
        }

        public void Save(IEnumerable<TInterface> items)
        {
            var entities = items.Cast<TImplementation>().ToList();
            foreach (var entity in entities)
            {
                if (entity.EntityId == null)
                {
                    entity.EntityId = Guid.NewGuid();
                }
            }

            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    connection.Execute(BulkInsertCommand, entities.Where(item => item.NeverPersisted), transaction);
                    connection.Execute(BulkUpdateCommand, entities.Where(item => !item.NeverPersisted && item.HasChanged), transaction);
                    transaction.Commit();
                }
            }

            SetAsPersisted(entities);
        }

        protected IDbConnection CreateConnection() => _container.Resolve<IConnectionFactoryRegistry>().GetConnection(_connectionFactoryName);

        protected IEnumerable<string> GetPersistedPropertyNames() => typeof(TImplementation).GetProperties().Where(property => property.CanRead && property.CanWrite && !_defaultIgnoredEntityProperties.Contains(property.Name) && !IgnoredEntityProperties.Contains(property.Name)).Select(property => property.Name);
    }
}

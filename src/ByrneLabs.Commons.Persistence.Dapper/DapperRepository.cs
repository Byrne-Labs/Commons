using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ByrneLabs.Commons.Domain;
using ByrneLabs.Commons.Ioc;
using ByrneLabs.Commons.Mapping;
using Dapper;
using Dapper.Contrib.Extensions;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Persistence.Dapper
{
    [PublicAPI]
    public abstract class DapperRepository<TDomainEntity, TDatabaseEntity> : Repository<TDomainEntity> where TDomainEntity : Entity where TDatabaseEntity : class
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
        private readonly object _lockSync = new object();
        private PropertyInfo _primaryKeyProperty;

        protected DapperRepository(string connectionFactoryName, IMapManager mapManager, IContainer container)
        {
            _container = container;
            MapManager = mapManager;
            _connectionFactoryName = connectionFactoryName;

            var map = new CustomPropertyTypeMap(typeof(TDomainEntity), (type, columnName) =>
            {
                PropertyInfo property;
                if (ColumnToPropertyMap.ContainsKey(columnName))
                {
                    property = typeof(TDomainEntity).GetProperty(ColumnToPropertyMap[columnName]);
                }
                else if (string.Equals(columnName, KeyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    property = typeof(TDomainEntity).GetProperty(nameof(Entity.EntityId));
                }
                else
                {
                    property = typeof(TDomainEntity).GetProperty(columnName);
                }

                return property;
            });
            SqlMapper.SetTypeMap(typeof(TDomainEntity), map);
        }

        protected virtual IDictionary<string, string> ColumnToPropertyMap { get; } = new Dictionary<string, string>();

        protected virtual IEnumerable<string> IgnoredEntityProperties { get; } = Enumerable.Empty<string>();

        protected virtual string InsertCommand
        {
            get
            {
                lock (_lockSync)
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

        protected virtual string KeyColumnName => typeof(TDatabaseEntity).Name + "Id";

        protected IMapManager MapManager { get; }

        protected virtual int? MaxReturnedRecords => null;

        protected virtual string SelectCommand => _defaultSelectCommand ??= $"SELECT * FROM {TableName}";

        protected virtual string TableName => typeof(TDatabaseEntity).Name;

        protected virtual string UpdateCommand
        {
            get
            {
                lock (_lockSync)
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

        private static IEnumerable<TDomainEntity> MarkAsPersisted(IEnumerable<TDomainEntity> entities)
        {
            foreach (var entity in entities)
            {
                entity.NeverPersisted = false;
                entity.HasChanged = false;
            }

            return entities;
        }

        public override void Delete(IEnumerable<TDomainEntity> entities)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            var failedDeletes = entities.Where(entity => !connection.Delete(entity, transaction));
            transaction.Commit();
            throw new PersistenceException("Some entities were not deleted", failedDeletes);
        }

        public override IEnumerable<TDomainEntity> Find(IEnumerable<Guid> entityIds)
        {
            using var connection = CreateConnection();
            connection.Open();
            var command = $"{SelectCommand} WHERE {KeyColumnName} IN @EntityIds";

            var databaseEntities = connection.Query<TDatabaseEntity>(command, new { EntityIds = entityIds.ToArray() }).ToList();
            return Convert(databaseEntities);
        }

        public virtual IEnumerable<TDomainEntity> Find(object criteria)
        {
            using var connection = CreateConnection();
            connection.Open();
            var criteriaFields = criteria.GetType().GetFields().Select(field => $"{field.Name} = @{field.Name}").Union(criteria.GetType().GetProperties().Where(property => property.CanRead).Select(property => $"{property.Name} = @{property.Name}")).ToList();
            var command = $"{SelectCommand} WHERE {string.Join(" AND ", criteriaFields)}";

            var databaseEntities = connection.Query<TDatabaseEntity>(command).ToList();
            return Convert(databaseEntities);
        }

        public override IEnumerable<TDomainEntity> FindAll()
        {
            using var connection = CreateConnection();
            connection.Open();
            var databaseEntities = connection.Query<TDatabaseEntity>(SelectCommand).ToList();
            return Convert(databaseEntities);
        }

        public override void Save(IEnumerable<TDomainEntity> entities)
        {
            foreach (var entity in entities.Where(entity => entity.EntityId == null))
            {
                entity.EntityId = Guid.NewGuid();
            }

            var databaseEntityMap = entities.Select(domainEntity => new Tuple<TDomainEntity, TDatabaseEntity>(domainEntity, Convert(domainEntity))).ToList();
            var insertDatabaseEntities = databaseEntityMap.Where(tuple => tuple.Item1.NeverPersisted).Select(tuple => tuple.Item2).ToList();
            var updateDatabaseEntities = databaseEntityMap.Where(tuple => !tuple.Item1.NeverPersisted && tuple.Item1.HasChanged).Select(tuple => tuple.Item2).ToList();

            using (var connection = CreateConnection())
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();
                connection.Execute(InsertCommand, insertDatabaseEntities, transaction);
                connection.Execute(UpdateCommand, updateDatabaseEntities, transaction);
                transaction.Commit();
            }

            foreach (var tuple in databaseEntityMap)
            {
                CopyData(tuple.Item2, tuple.Item1);
            }

            MarkAsPersisted(entities);
        }

        protected TDatabaseEntity Convert(TDomainEntity domainEntity) => domainEntity == null ? null : Convert(new[] { domainEntity }).First();

        protected TDomainEntity Convert(TDatabaseEntity databaseEntity) => databaseEntity == null ? null : Convert(new[] { databaseEntity }).First();

        protected virtual IEnumerable<TDomainEntity> Convert(IEnumerable<TDatabaseEntity> databaseEntities)
        {
            if (MaxReturnedRecords != null && MaxReturnedRecords > 0)
            {
                databaseEntities = databaseEntities.Take(MaxReturnedRecords.Value);
            }

            var domainEntities = MapManager.Map<TDatabaseEntity, TDomainEntity>(databaseEntities);
            if (domainEntities.Any())
            {
                FillInDomainEntities(domainEntities);
            }

            return MarkAsPersisted(domainEntities).ToList();
        }

        protected virtual IEnumerable<TDatabaseEntity> Convert(IEnumerable<TDomainEntity> domainEntities)
        {
            var databaseEntities = MapManager.Map<TDomainEntity, TDatabaseEntity>(domainEntities);
            if (databaseEntities.Any())
            {
                FillInDatabaseEntities(databaseEntities);
            }

            return databaseEntities.ToList();
        }

        protected virtual void CopyData(TDomainEntity domainEntity, TDatabaseEntity databaseEntity)
        {
            MapManager.Map(domainEntity, databaseEntity);
            FillInDatabaseEntities(new[] { databaseEntity });
        }

        protected virtual void CopyData(TDatabaseEntity databaseEntity, TDomainEntity domainEntity)
        {
            MapManager.Map(databaseEntity, domainEntity);
            FillInDomainEntities(new[] { domainEntity });
        }

        protected IDbConnection CreateConnection() => _container.Resolve<IConnectionFactoryRegistry>().GetConnection(_connectionFactoryName);

        [SuppressMessage("ReSharper", "UnusedParameter.Global", Justification = "This method by default does nothing but it is important to allow inheriting classes to override it")]
        protected virtual void FillInDatabaseEntities(IEnumerable<TDatabaseEntity> databaseEntities)
        {
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Global", Justification = "This method by default does nothing but it is important to allow inheriting classes to override it")]
        protected virtual void FillInDomainEntities(IEnumerable<TDomainEntity> domainEntities)
        {
        }

        protected virtual IQueryable<TDatabaseEntity> GetMatchingDatabaseEntities(IEnumerable<Guid> domainEntityIds, IQueryable<TDatabaseEntity> databaseEntities)
        {
            return databaseEntities.Where(databaseEntity => domainEntityIds.Contains(GetDomainEntityId(databaseEntity)));
        }

        protected IEnumerable<string> GetPersistedPropertyNames() => typeof(TDomainEntity).GetProperties().Where(property => property.CanRead && property.CanWrite && !_defaultIgnoredEntityProperties.Contains(property.Name) && !IgnoredEntityProperties.Contains(property.Name)).Select(property => property.Name);

        private Guid GetDomainEntityId(TDatabaseEntity databaseEntity)
        {
            if (_primaryKeyProperty == null)
            {
                var keyAttributes = typeof(TDatabaseEntity).GetPropertiesWithCustomAttribute<KeyAttribute>().Union(typeof(TDatabaseEntity).GetPropertiesWithCustomAttribute<ExplicitKeyAttribute>());
                if (keyAttributes.Count() != 1)
                {
                    throw new InvalidOperationException($"The type {typeof(TDatabaseEntity).FullName} must have exactly one property decorated by {typeof(KeyAttribute).FullName} or {typeof(ExplicitKeyAttribute).FullName}");
                }

                _primaryKeyProperty = keyAttributes.First();
            }

            Guid domainEntityId;
            if (_primaryKeyProperty.PropertyType == typeof(Guid))
            {
                domainEntityId = (Guid) _primaryKeyProperty.GetValue(databaseEntity);
            }
            else if (_primaryKeyProperty.PropertyType == typeof(int))
            {
                domainEntityId = ((int) _primaryKeyProperty.GetValue(databaseEntity)).StoreAsGuid();
            }
            else if (_primaryKeyProperty.PropertyType == typeof(long))
            {
                domainEntityId = ((long) _primaryKeyProperty.GetValue(databaseEntity)).StoreAsGuid();
            }
            else if (_primaryKeyProperty.PropertyType == typeof(short))
            {
                domainEntityId = ((short) _primaryKeyProperty.GetValue(databaseEntity)).StoreAsGuid();
            }
            else if (_primaryKeyProperty.PropertyType == typeof(byte))
            {
                domainEntityId = ((byte) _primaryKeyProperty.GetValue(databaseEntity)).StoreAsGuid();
            }
            else if (_primaryKeyProperty.PropertyType == typeof(string))
            {
                domainEntityId = ((string) _primaryKeyProperty.GetValue(databaseEntity)).StoreAsGuid();
            }
            else
            {
                throw new InvalidOperationException($"The property {typeof(TDatabaseEntity).FullName}.{_primaryKeyProperty.Name} cannot be of type {_primaryKeyProperty.PropertyType.FullName}");
            }

            return domainEntityId;
        }
    }
}

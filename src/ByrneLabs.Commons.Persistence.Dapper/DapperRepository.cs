using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        protected DapperRepository(string connectionFactoryName, IContainer container)
        {
            _container = container;
            _connectionFactoryName = connectionFactoryName;

            var map = new CustomPropertyTypeMap(typeof(TDatabaseEntity), (type, columnName) =>
            {
                PropertyInfo property;
                if (ColumnToPropertyMap.ContainsKey(columnName))
                {
                    property = typeof(TDatabaseEntity).GetProperty(ColumnToPropertyMap[columnName]);
                }
                else if (string.Equals(columnName, KeyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    property = typeof(TDatabaseEntity).GetProperty(nameof(Entity.EntityId));
                }
                else
                {
                    property = typeof(TDatabaseEntity).GetProperty(columnName);
                }

                return property;
            });
            SqlMapper.SetTypeMap(typeof(TDatabaseEntity), map);
        }

        protected virtual IDictionary<string, string> ColumnToPropertyMap { get; } = new Dictionary<string, string>();

        protected virtual bool DatabaseGeneratedPrimaryKey => true;

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

        protected virtual int? MaxReturnedRecords => null;

        protected virtual string SelectCommand
        {
            get
            {
                string selectCommand;
                if (!string.IsNullOrWhiteSpace(_defaultSelectCommand))
                {
                    selectCommand = _defaultSelectCommand;
                }
                else
                {
                    var properties = GetPersistedPropertyNames();
                    var columns = string.Join(", ", properties);
                    selectCommand = $"SELECT {KeyColumnName} AS EntityId, {columns} FROM {TableName}";
                }

                return selectCommand;
            }
        }

        protected virtual string TableName => typeof(TDatabaseEntity).Name;

        protected virtual string UpdateCommand
        {
            get
            {
                lock (_lockSync)
                {
                    if (_defaultBulkUpdateCommand == null)
                    {
                        var properties = GetPersistedPropertyNames().Where(property => property != KeyColumnName).Select(property => property + " = " + "@" + property);
                        _defaultBulkUpdateCommand = $"UPDATE {TableName} SET {string.Join(", ", properties)} WHERE {KeyColumnName} = @EntityId";
                    }

                    return _defaultBulkUpdateCommand;
                }
            }
        }

        private static void MarkAsPersisted(IEnumerable<TDomainEntity> entities)
        {
            foreach (var entity in entities)
            {
                entity.NeverPersisted = false;
                entity.HasChanged = false;
            }
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
            var entityIdArray = entityIds.ToArray();
            var command = $"{SelectCommand} WHERE {KeyColumnName} IN @EntityIds";

            var queryBatches = new List<IEnumerable<Guid>>();
            var start = 0;
            while (start < entityIdArray.Length)
            {
                var queryBatch = entityIdArray.Skip(start).Take(2000).ToArray();
                queryBatches.Add(queryBatch);
                start += 2000;
            }

            var databaseEntities = new ConcurrentBag<TDatabaseEntity>();

            Parallel.ForEach(queryBatches, queryBatch =>
            {
                using var connection = CreateConnection();
                connection.Open();
                var queryResults = connection.Query<TDatabaseEntity>(command, new { EntityIds = queryBatch }).ToList();
                foreach (var queryResult in queryResults)
                {
                    databaseEntities.Add(queryResult);
                }
            });

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
            var entityArray = entities.ToArray();
            foreach (var entity in entityArray.Where(entity => entity.EntityId == null))
            {
                entity.EntityId = Guid.NewGuid();
            }

            var databaseEntityMap = entityArray.Select(domainEntity => new Tuple<TDomainEntity, TDatabaseEntity>(domainEntity, Convert(domainEntity))).ToList();
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

            MarkAsPersisted(entityArray);
        }

        protected TDatabaseEntity Convert(TDomainEntity domainEntity) => domainEntity == null ? null : Convert(new[] { domainEntity }).First();

        protected TDomainEntity Convert(TDatabaseEntity databaseEntity) => databaseEntity == null ? null : Convert(new[] { databaseEntity }).First();

        protected virtual IEnumerable<TDomainEntity> Convert(IEnumerable<TDatabaseEntity> databaseEntities)
        {
            if (MaxReturnedRecords != null && MaxReturnedRecords > 0)
            {
                databaseEntities = databaseEntities.Take(MaxReturnedRecords.Value);
            }

            var mapManager = _container.Resolve<IMapManager>();
            var domainEntities = mapManager.Map<TDatabaseEntity, TDomainEntity>(databaseEntities).ToList();
            if (domainEntities.Any())
            {
                FillInDomainEntities(domainEntities);
            }

            MarkAsPersisted(domainEntities);

            return domainEntities;
        }

        protected virtual IEnumerable<TDatabaseEntity> Convert(IEnumerable<TDomainEntity> domainEntities)
        {
            var mapManager = _container.Resolve<IMapManager>();
            var databaseEntities = mapManager.Map<TDomainEntity, TDatabaseEntity>(domainEntities).ToList();
            if (databaseEntities.Any())
            {
                FillInDatabaseEntities(databaseEntities);
            }

            return databaseEntities;
        }

        protected virtual void CopyData(TDomainEntity domainEntity, TDatabaseEntity databaseEntity)
        {
            var mapManager = _container.Resolve<IMapManager>();
            mapManager.Map(domainEntity, databaseEntity);
            FillInDatabaseEntities(new[] { databaseEntity });
        }

        protected virtual void CopyData(TDatabaseEntity databaseEntity, TDomainEntity domainEntity)
        {
            var mapManager = _container.Resolve<IMapManager>();
            mapManager.Map(databaseEntity, domainEntity);
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

        protected virtual IEnumerable<TDomainEntity> FindByExample(object criteria)
        {
            using var connection = CreateConnection();
            connection.Open();
            var criteriaFields = criteria.GetType().GetFields().Select(field => $"{field.Name} = @{field.Name}").Union(criteria.GetType().GetProperties().Where(property => property.CanRead).Select(property => $"{property.Name} = @{property.Name}")).ToList();
            var command = $"{SelectCommand} WHERE {string.Join(" AND ", criteriaFields)}";

            var databaseEntities = connection.Query<TDatabaseEntity>(command, criteria).ToList();
            return Convert(databaseEntities);
        }

        protected virtual IEnumerable<TDomainEntity> FindWhere(string whereClause, object parameterValues)
        {
            using var connection = CreateConnection();
            connection.Open();
            var command = $"{SelectCommand} WHERE {whereClause}";

            var databaseEntities = connection.Query<TDatabaseEntity>(command, parameterValues).ToList();
            return Convert(databaseEntities);
        }

        protected virtual IQueryable<TDatabaseEntity> GetMatchingDatabaseEntities(IEnumerable<Guid> domainEntityIds, IQueryable<TDatabaseEntity> databaseEntities)
        {
            return databaseEntities.Where(databaseEntity => domainEntityIds.Contains(GetDomainEntityId(databaseEntity)));
        }

        protected IList<string> GetPersistedPropertyNames() => typeof(TDatabaseEntity).GetProperties().Where(property =>
            property.CanRead &&
            property.CanWrite &&
            (!DatabaseGeneratedPrimaryKey || property.Name != KeyColumnName) &&
            !_defaultIgnoredEntityProperties.Contains(property.Name) &&
            !IgnoredEntityProperties.Contains(property.Name)).Select(property => property.Name).ToList();

        private Guid GetDomainEntityId(TDatabaseEntity databaseEntity)
        {
            if (_primaryKeyProperty == null)
            {
                var keyAttributes = typeof(TDatabaseEntity).GetPropertiesWithCustomAttribute<KeyAttribute>().Union(typeof(TDatabaseEntity).GetPropertiesWithCustomAttribute<ExplicitKeyAttribute>()).ToList();
                if (keyAttributes.Count != 1)
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

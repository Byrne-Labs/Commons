using System;
using System.Collections.Concurrent;
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
    public abstract class DapperRepositoryWithMapping<TDomainEntity, TDatabaseEntity> : Repository<TDomainEntity> where TDomainEntity : Entity where TDatabaseEntity : Entity
    {
        private readonly IContainer _container;
        private readonly string[] _defaultIgnoredEntityProperties = { nameof(Entity.EntityId), nameof(Entity.NeverPersisted), nameof(Entity.HasChanged) };
        private PropertyInfo _primaryKeyProperty;

        protected DapperRepositoryWithMapping(IContainer container)
        {
            _container = container;

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
                var properties = GetPersistedPropertyNames();
                var columns = string.Join(", ", properties);
                var parameters = string.Join(", ", properties.Select(property => "@" + property));
                return $"INSERT {TableName} ({KeyColumnName}, {columns}) VALUES (@EntityId, {parameters})";
            }
        }

        protected virtual string KeyColumnName => typeof(TDatabaseEntity).Name + "Id";

        protected virtual int? MaxReturnedRecords => null;

        protected virtual string SelectCommand
        {
            get
            {
                var properties = GetPersistedPropertyNames();
                var columns = string.Join(", ", properties);
                return $"SELECT {KeyColumnName} AS EntityId, {columns} FROM {TableName}";
            }
        }

        protected virtual string TableName => typeof(TDatabaseEntity).Name;

        protected virtual string UpdateCommand
        {
            get
            {
                var properties = GetPersistedPropertyNames().Where(property => property != KeyColumnName).Select(property => property + " = " + "@" + property);
                return $"UPDATE {TableName} SET {string.Join(", ", properties)} WHERE {KeyColumnName} = @EntityId";
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
            var entitiesArray = entities.ToArray();
            if (entitiesArray?.Any() != true)
            {
                return;
            }

            var databaseEntities = Convert(entitiesArray);
            var connection = GetConnection();
            using var transaction = connection.BeginTransaction();
            var failedDeletes = databaseEntities.Where(databaseEntity => !connection.Delete(databaseEntity, transaction)).ToArray();
            transaction.Commit();
            if (failedDeletes.Any())
            {
                throw new PersistenceException("Some entities were not deleted", failedDeletes);
            }
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

            var connection = GetConnection();
            foreach (var queryBatch in queryBatches)
            {
                var queryResults = connection.Query<TDatabaseEntity>(command, new { EntityIds = queryBatch }).ToArray();
                foreach (var queryResult in queryResults)
                {
                    databaseEntities.Add(queryResult);
                }
            }

            return Convert(databaseEntities);
        }

        public override IEnumerable<TDomainEntity> FindAll()
        {
            var connection = GetConnection();
            var databaseEntities = connection.Query<TDatabaseEntity>(SelectCommand).ToArray();
            return Convert(databaseEntities);
        }

        public override void Save(IEnumerable<TDomainEntity> entities)
        {
            var entityArray = entities.ToArray();
            foreach (var entity in entityArray.Where(entity => entity.EntityId == null))
            {
                entity.EntityId = Guid.NewGuid();
            }

            var databaseEntityMap = entityArray.Select(domainEntity => new Tuple<TDomainEntity, TDatabaseEntity>(domainEntity, Convert(domainEntity))).ToArray();
            var insertDatabaseEntities = databaseEntityMap.Where(tuple => tuple.Item1.NeverPersisted).Select(tuple => tuple.Item2).ToArray();
            var updateDatabaseEntities = databaseEntityMap.Where(tuple => !tuple.Item1.NeverPersisted && tuple.Item1.HasChanged).Select(tuple => tuple.Item2).ToArray();

            using (var connection = GetConnection())
            {
                using var transaction = connection.BeginTransaction();
                connection.Execute(InsertCommand, insertDatabaseEntities, transaction);
                connection.Execute(UpdateCommand, updateDatabaseEntities, transaction);
                transaction.Commit();
            }

            MarkAsPersisted(entityArray);
        }

        protected abstract IDbConnection GetConnection();

        protected TDatabaseEntity Convert(TDomainEntity domainEntity) => domainEntity == null ? null : Convert(new[] { domainEntity }).First();

        protected TDomainEntity Convert(TDatabaseEntity databaseEntity) => databaseEntity == null ? null : Convert(new[] { databaseEntity }).First();

        protected virtual IEnumerable<TDomainEntity> Convert(IEnumerable<TDatabaseEntity> databaseEntities)
        {
            if (MaxReturnedRecords != null && MaxReturnedRecords > 0)
            {
                databaseEntities = databaseEntities.Take(MaxReturnedRecords.Value);
            }

            var mapManager = _container.Resolve<IMapManager>();
            var domainEntities = mapManager.Map<TDatabaseEntity, TDomainEntity>(databaseEntities).ToArray();
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
            var databaseEntities = mapManager.Map<TDomainEntity, TDatabaseEntity>(domainEntities).ToArray();
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
            var connection = GetConnection();
            var criteriaFields = criteria.GetType().GetFields().Select(field => $"{field.Name} = @{field.Name}").Union(criteria.GetType().GetProperties().Where(property => property.CanRead).Select(property => $"{property.Name} = @{property.Name}")).ToArray();
            var command = $"{SelectCommand} WHERE {string.Join(" AND ", criteriaFields)}";

            var databaseEntities = connection.Query<TDatabaseEntity>(command, criteria).ToArray();
            return Convert(databaseEntities);
        }

        protected virtual IEnumerable<TDomainEntity> FindWhere(string whereClause, object parameterValues)
        {
            var connection = GetConnection();
            var command = $"{SelectCommand} WHERE {whereClause}";

            var databaseEntities = connection.Query<TDatabaseEntity>(command, parameterValues).ToArray();
            return Convert(databaseEntities);
        }

        protected virtual IEnumerable<TDomainEntity> FindWhereIn(string columnName, IEnumerable<object> parameterValues)
        {
            var parameterValuesArray = parameterValues.ToArray();
            var command = $"{SelectCommand} WHERE {columnName} IN @Values";

            var queryBatches = new List<IEnumerable<object>>();
            var start = 0;
            while (start < parameterValuesArray.Length)
            {
                var queryBatch = parameterValuesArray.Skip(start).Take(2000).ToArray();
                queryBatches.Add(queryBatch);
                start += 2000;
            }

            var databaseEntities = new ConcurrentBag<TDatabaseEntity>();

            var connection = GetConnection();
            foreach (var queryBatch in queryBatches)
            {
                var queryResults = connection.Query<TDatabaseEntity>(command, new { Values = queryBatch }).ToArray();
                foreach (var queryResult in queryResults)
                {
                    databaseEntities.Add(queryResult);
                }
            }

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
            !IgnoredEntityProperties.Contains(property.Name)).Select(property => property.Name).ToArray();

        private Guid GetDomainEntityId(TDatabaseEntity databaseEntity)
        {
            if (_primaryKeyProperty == null)
            {
                var keyAttributes = typeof(TDatabaseEntity).GetPropertiesWithCustomAttribute<KeyAttribute>().Union(typeof(TDatabaseEntity).GetPropertiesWithCustomAttribute<ExplicitKeyAttribute>()).ToArray();
                if (keyAttributes.Length != 1)
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

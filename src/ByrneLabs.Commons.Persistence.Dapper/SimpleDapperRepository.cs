using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ByrneLabs.Commons.Domain;
using Dapper;
using Dapper.Contrib.Extensions;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Persistence.Dapper
{
    [PublicAPI]
    public abstract class SimpleDapperRepository<T> : Repository<T> where T : Entity
    {
        private readonly string[] _defaultIgnoredEntityProperties = { nameof(Entity.EntityId), nameof(Entity.NeverPersisted), nameof(Entity.HasChanged) };

        protected SimpleDapperRepository()
        {
            var map = new CustomPropertyTypeMap(typeof(T), (type, columnName) =>
            {
                PropertyInfo property;
                if (ColumnToPropertyMap.ContainsKey(columnName))
                {
                    property = typeof(T).GetProperty(ColumnToPropertyMap[columnName]);
                }
                else if (string.Equals(columnName, KeyColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    property = typeof(T).GetProperty(nameof(Entity.EntityId));
                }
                else
                {
                    property = typeof(T).GetProperty(columnName);
                }

                return property;
            });
            SqlMapper.SetTypeMap(typeof(T), map);
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

        protected virtual string KeyColumnName => typeof(T).Name + "Id";

        protected virtual string SelectCommand
        {
            get
            {
                var properties = GetPersistedPropertyNames();
                var columns = string.Join(", ", properties);
                return $"SELECT {KeyColumnName} AS EntityId, {columns} FROM {TableName}";
            }
        }

        protected virtual string TableName => typeof(T).Name;

        protected virtual string UpdateCommand
        {
            get
            {
                var properties = GetPersistedPropertyNames().Where(property => property != KeyColumnName).Select(property => property + " = " + "@" + property);
                return $"UPDATE {TableName} SET {string.Join(", ", properties)} WHERE {KeyColumnName} = @EntityId";
            }
        }

        private static void MarkAsPersisted(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entity.NeverPersisted = false;
                entity.HasChanged = false;
            }
        }

        public override void Delete(IEnumerable<T> entities)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            var failedDeletes = entities.Where(entity => !connection.Delete(entity, transaction));
            transaction.Commit();
            throw new PersistenceException("Some entities were not deleted", failedDeletes);
        }

        public override IEnumerable<T> Find(IEnumerable<Guid> entityIds)
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

            var entities = new ConcurrentBag<T>();

            Parallel.ForEach(queryBatches, queryBatch =>
            {
                using var connection = CreateConnection();
                connection.Open();
                var queryResults = connection.Query<T>(command, new { EntityIds = queryBatch }).ToArray();
                foreach (var queryResult in queryResults)
                {
                    entities.Add(queryResult);
                }
            });

            return entities.ToArray();
        }

        public override IEnumerable<T> FindAll()
        {
            using var connection = CreateConnection();
            connection.Open();
            return connection.Query<T>(SelectCommand).ToArray();
        }

        public override void Save(IEnumerable<T> entities)
        {
            var entityArray = entities.ToArray();
            foreach (var entity in entityArray.Where(entity => entity.EntityId == null))
            {
                entity.EntityId = Guid.NewGuid();
            }

            var insertEntities = entityArray.Where(entity => entity.NeverPersisted).ToArray();
            var updateEntities = entityArray.Where(entity => !entity.NeverPersisted && entity.HasChanged);

            using (var connection = CreateConnection())
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();
                connection.Execute(InsertCommand, insertEntities, transaction);
                connection.Execute(UpdateCommand, updateEntities, transaction);
                transaction.Commit();
            }

            MarkAsPersisted(entityArray);
        }

        protected abstract IDbConnection CreateConnection();

        protected virtual IEnumerable<T> FindByExample(object criteria)
        {
            using var connection = CreateConnection();
            connection.Open();
            var criteriaFields = criteria.GetType().GetFields().Select(field => $"{field.Name} = @{field.Name}").Union(criteria.GetType().GetProperties().Where(property => property.CanRead).Select(property => $"{property.Name} = @{property.Name}")).ToArray();
            var command = $"{SelectCommand} WHERE {string.Join(" AND ", criteriaFields)}";

            return connection.Query<T>(command, criteria).ToArray();
        }

        protected virtual IEnumerable<T> FindWhere(string whereClause, object parameterValues)
        {
            using var connection = CreateConnection();
            connection.Open();
            var command = $"{SelectCommand} WHERE {whereClause}";

            return connection.Query<T>(command, parameterValues).ToArray();
        }

        protected virtual IEnumerable<T> FindWhereIn(string columnName, IEnumerable<object> parameterValues)
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

            var entities = new ConcurrentBag<T>();

            Parallel.ForEach(queryBatches, queryBatch =>
            {
                using var connection = CreateConnection();
                connection.Open();
                var queryResults = connection.Query<T>(command, new { Values = queryBatch }).ToArray();
                foreach (var queryResult in queryResults)
                {
                    entities.Add(queryResult);
                }
            });

            return entities.ToArray();
        }

        protected IList<string> GetPersistedPropertyNames() => typeof(T).GetProperties().Where(property =>
            property.CanRead &&
            property.CanWrite &&
            (!DatabaseGeneratedPrimaryKey || property.Name != KeyColumnName) &&
            !_defaultIgnoredEntityProperties.Contains(property.Name) &&
            !IgnoredEntityProperties.Contains(property.Name)).Select(property => property.Name).ToArray();
    }
}

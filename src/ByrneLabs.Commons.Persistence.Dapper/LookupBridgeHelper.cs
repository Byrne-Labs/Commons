using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ByrneLabs.Commons.Domain;
using ByrneLabs.Commons.Ioc;
using Dapper;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Persistence.Dapper
{
    [PublicAPI]
    public abstract class LookupBridgeHelper<TConsumer, TLookup> where TConsumer : Entity where TLookup : Entity
    {
        private readonly string _connectionFactoryName;
        private readonly IContainer _container;

        protected LookupBridgeHelper(string connectionFactoryName, IContainer container)
        {
            _connectionFactoryName = connectionFactoryName;
            _container = container;
        }

        protected virtual string ConsumerEntityIdFieldName => $"{typeof(TLookup).Name}Id";

        protected virtual string InsertCommand => $"INSERT {TableName} ({KeyColumnName}, {ConsumerEntityIdFieldName}, {LookupEntityIdFieldName}) VALUES (@LookupBridgeId, @ConsumerId, @LookupId)";

        protected virtual string KeyColumnName => $"{TableName}Id";

        protected virtual string LookupEntityIdFieldName => $"{typeof(TLookup).Name}Id";

        protected virtual string SelectCommand => $"SELECT {KeyColumnName} AS LookupBridgeId, {ConsumerEntityIdFieldName} AS ConsumerId, {LookupEntityIdFieldName} AS LookupId FROM {TableName}";

        protected virtual string TableName => $"{typeof(TConsumer).Name}{typeof(TLookup).Name}";

        public IEnumerable<LookupBridge> Find(IEnumerable<Guid> consumersIds)
        {
            var consumersIdsArray = consumersIds.ToArray();
            var command = $"{SelectCommand} WHERE {ConsumerEntityIdFieldName} IN @ConsumerEntityIds";

            var queryBatches = new List<IEnumerable<Guid>>();
            var start = 0;
            while (start < consumersIdsArray.Length)
            {
                var queryBatch = consumersIdsArray.Skip(start).Take(2000).ToArray();
                queryBatches.Add(queryBatch);
                start += 2000;
            }

            var bridgeEntities = new ConcurrentBag<LookupBridge>();

            Parallel.ForEach(queryBatches, queryBatch =>
            {
                using var connection = CreateConnection();
                connection.Open();
                var queryResults = connection.Query<LookupBridge>(command, new { ConsumerEntityIds = queryBatch }).ToArray();
                foreach (var queryResult in queryResults)
                {
                    bridgeEntities.Add(queryResult);
                }
            });

            return bridgeEntities;
        }

        public void Save(IEnumerable<Tuple<TConsumer, IEnumerable<TLookup>>> relationships)
        {
            Parallel.ForEach(relationships, relationship =>
            {
                using var connection = CreateConnection();
                connection.Open();
                var existingBridges = connection.Query<LookupBridge>($"{SelectCommand} WHERE {ConsumerEntityIdFieldName} = @ConsumerEntityId", new { ConsumerEntityId = relationship.Item1.EntityId.Value }).ToArray();

                var bridgesToRemove = existingBridges.Where(existingBridge => relationship.Item2.All(newLookup => existingBridge.LookupId != newLookup.EntityId.Value)).ToArray();
                var bridgesToAdd = relationship.Item2
                    .Where(newLookup => existingBridges.All(existingBridge => existingBridge.LookupId != newLookup.EntityId.Value))
                    .Select(newLookup => new LookupBridge { LookupBridgeId = Guid.NewGuid(), ConsumerId = relationship.Item1.EntityId.Value, LookupId = newLookup.EntityId.Value })
                    .ToArray();

                using var transaction = connection.BeginTransaction();
                connection.Execute($"DELETE {TableName} WHERE {KeyColumnName} IN @LookupBridgeIds", new { LookupBridgeIds = bridgesToRemove.Select(bridgeToRemove => bridgeToRemove.LookupBridgeId) }, transaction);
                connection.Execute(InsertCommand, bridgesToAdd, transaction);
                transaction.Commit();
            });
        }

        protected IDbConnection CreateConnection() => _container.Resolve<IConnectionFactoryRegistry>().GetConnection(_connectionFactoryName);
    }
}

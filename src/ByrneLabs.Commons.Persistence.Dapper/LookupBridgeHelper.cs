using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ByrneLabs.Commons.Domain;
using Dapper;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Persistence.Dapper
{
    [PublicAPI]
    public abstract class LookupBridgeHelper<TConsumer, TLookup> where TConsumer : Entity where TLookup : Entity
    {
        protected virtual string ConsumerEntityIdFieldName => $"{typeof(TConsumer).Name}Id";

        protected virtual string InsertCommand => $"INSERT {TableName} ({KeyColumnName}, {ConsumerEntityIdFieldName}, {LookupEntityIdFieldName}) VALUES (@LookupBridgeId, @ConsumerId, @LookupId)";

        protected virtual string KeyColumnName => $"{TableName}Id";

        protected virtual string LookupEntityIdFieldName => $"{typeof(TLookup).Name}Id";

        protected virtual string SelectCommand => $"SELECT {KeyColumnName} AS LookupBridgeId, {ConsumerEntityIdFieldName} AS ConsumerId, {LookupEntityIdFieldName} AS LookupId FROM {TableName}";

        protected virtual string TableName => $"{typeof(TConsumer).Name}{typeof(TLookup).Name}";

        public void DeleteLookupsForConsumers(IEnumerable<Guid> consumerIds)
        {
            var consumersIdsArray = consumerIds.ToArray();

            var queryBatches = new List<IEnumerable<Guid>>();
            var start = 0;
            while (start < consumersIdsArray.Length)
            {
                var queryBatch = consumersIdsArray.Skip(start).Take(2000).ToArray();
                queryBatches.Add(queryBatch);
                start += 2000;
            }

            var connection = OpenConnection();
            foreach (var queryBatch in queryBatches)
            {
                connection.Execute($"DELETE {TableName} WHERE {ConsumerEntityIdFieldName} IN @ConsumerIds", new { ConsumerIds = queryBatch });
            }
        }

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

            var connection = OpenConnection();
            foreach (var queryBatch in queryBatches)
            {
                var queryResults = connection.Query<LookupBridge>(command, new { ConsumerEntityIds = queryBatch }).ToArray();
                foreach (var queryResult in queryResults)
                {
                    bridgeEntities.Add(queryResult);
                }
            }

            return bridgeEntities;
        }

        public void Save(IEnumerable<Tuple<TConsumer, IEnumerable<TLookup>>> relationships)
        {
            var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();
            foreach (var relationship in relationships)
            {
                var existingBridges = connection.Query<LookupBridge>($"{SelectCommand} WHERE {ConsumerEntityIdFieldName} = @ConsumerEntityId", new { ConsumerEntityId = relationship.Item1.EntityId.Value }).ToArray();

                var bridgesToRemove = existingBridges.Where(existingBridge => relationship.Item2.All(newLookup => existingBridge.LookupId != newLookup.EntityId.Value)).ToArray();
                var bridgesToAdd = relationship.Item2
                    .Where(newLookup => existingBridges.All(existingBridge => existingBridge.LookupId != newLookup.EntityId.Value))
                    .Select(newLookup => new LookupBridge { LookupBridgeId = Guid.NewGuid(), ConsumerId = relationship.Item1.EntityId.Value, LookupId = newLookup.EntityId.Value })
                    .ToArray();

                connection.Execute($"DELETE {TableName} WHERE {KeyColumnName} IN @LookupBridgeIds", new { LookupBridgeIds = bridgesToRemove.Select(bridgeToRemove => bridgeToRemove.LookupBridgeId) }, transaction);
                connection.Execute(InsertCommand, bridgesToAdd, transaction);
            }

            transaction.Commit();
        }

        protected abstract IDbConnection OpenConnection();
    }
}

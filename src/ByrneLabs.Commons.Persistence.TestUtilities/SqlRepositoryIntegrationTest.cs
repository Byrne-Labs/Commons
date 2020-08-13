using System;
using System.Collections.Generic;
using System.Linq;
using ByrneLabs.Commons.Domain;
using ByrneLabs.Commons.Ioc;
using ByrneLabs.Commons.TestUtilities;
using ByrneLabs.Commons.TestUtilities.XUnit;
using JetBrains.Annotations;
using Xunit;

namespace ByrneLabs.Commons.Persistence.TestUtilities
{
    [PublicAPI]
    public abstract class SqlRepositoryIntegrationTest<TRepositoryInterface, TEntity> : SqlIntegrationTest where TRepositoryInterface : class, IRepository<TEntity> where TEntity : IEntity
    {
        protected virtual string TableName
        {
            get
            {
                var entityType = typeof(TEntity);
                return entityType.IsInterface && entityType.Name.Length > 1 && entityType.Name.Substring(0, 2).IsAllUpper() ? entityType.Name.Substring(1) : entityType.Name;
            }
        }

        protected static void AssertValid(IEnumerable<TEntity> entities) => AssertValid(entities.Cast<IEntity>().ToList());

        [Fact]
        [Trait("Test Type", "Integration Test")]
        public virtual void IntegrationTestFindAll()
        {
            using var testHelper = GetNewRepositoryTestHelper();
            var testEntities = testHelper.TestData<TEntity>();
            testHelper.TestedObject.Save(testEntities);

            var entityIds = testHelper.TestData<TEntity>().Select(entity => entity.EntityId.Value).Distinct().ToArray();
            var foundEntities = testHelper.TestedObject.FindAll().ToArray();
            AssertValid(foundEntities);
            foreach (var foundEntity in foundEntities)
            {
                Assert.Contains(foundEntity.EntityId.Value, entityIds);
            }

            var foundEntityIds = foundEntities.Select(entity => entity.EntityId.Value);
            foreach (var entityId in entityIds)
            {
                Assert.Contains(entityId, foundEntityIds);
            }
        }

        [Fact]
        [Trait("Test Type", "Integration Test")]
        public virtual void IntegrationTestFindById()
        {
            using var testHelper = GetNewRepositoryTestHelper();
            var testEntities = testHelper.TestData<TEntity>();
            testHelper.TestedObject.Save(testEntities);

            var entityIds = GetEntityIds(testHelper.Container);
            foreach (var entityId in entityIds)
            {
                var entity = testHelper.TestedObject.Find(entityId);
                AssertValid(entity);
                Assert.Equal(entityId, entity.EntityId.Value);
            }
        }

        [Fact]
        [Trait("Test Type", "Integration Test")]
        public virtual void IntegrationTestFindByIds()
        {
            using var testHelper = GetNewRepositoryTestHelper();
            var testEntities = testHelper.TestData<TEntity>();
            testHelper.TestedObject.Save(testEntities);

            var entityIds = GetEntityIds(testHelper.Container);
            var entities = testHelper.TestedObject.Find(entityIds);
            AssertValid(entities);
            BetterAssert.ContainsSame(entityIds, entities.Select(entity => entity.EntityId.Value).ToArray());
        }

        [Fact]
        [Trait("Test Type", "Integration Test")]
        public void IntegrationTestSave()
        {
            using var testHelper = GetNewRepositoryTestHelper();
            var testEntities = testHelper.TestData<TEntity>();
            testHelper.TestedObject.Save(testEntities);
            AssertValid(testHelper.TestData<TEntity>());
        }

        protected abstract ITestHelper<TRepositoryInterface> GetNewRepositoryTestHelper();

        protected virtual Guid CreateEntityId(params object[] primaryKeys) => (Guid) primaryKeys[0];

        protected virtual string CreateQueryForPrimaryKeys() => $"SELECT {TableName}Id FROM {TableName}";

        protected virtual IEnumerable<Guid> GetEntityIds(IContainer container)
        {
            var entityIds = new List<Guid>();
            using (var connection = container.Resolve<IConnectionFactoryRegistry>().GetConnection(ConnectionName))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = CreateQueryForPrimaryKeys();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var fieldValues = new object[reader.FieldCount];
                    reader.GetValues(fieldValues);
                    var entityId = CreateEntityId(fieldValues);
                    entityIds.Add(entityId);
                }

                connection.Close();
            }

            Assert.NotEmpty(entityIds);

            return entityIds;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
    public abstract class SqlRepositoryIntegrationTest<TRepositoryInterface, TEntity> : IDisposable where TRepositoryInterface : class, IRepository<TEntity> where TEntity : IEntity
    {
        private readonly object _lockSync = new object();

        protected abstract string ConnectionName { get; }

        protected abstract string EmptyTestDatabaseFilePath { get; }

        protected static void AssertValid(IEntity entity) => AssertValid(new[] { entity });

        protected static void AssertValid(IEnumerable<TEntity> entities) => AssertValid(entities.Cast<IEntity>().ToList());

        protected static void AssertValid(IEnumerable<IEntity> entities, IList<IEntity> examinedEntities = null)
        {
            Assert.NotNull(entities);
            Assert.NotEmpty(entities);

            if (examinedEntities == null)
            {
                examinedEntities = new List<IEntity>();
            }

            foreach (var entity in entities.Where(e => !examinedEntities.Contains(e)))
            {
                Assert.NotNull(entity?.EntityId);

                examinedEntities.Add(entity);

                var otherEntities = new List<IEntity>();
                foreach (var property in entity.GetType().GetProperties().Where(p => p.CanRead))
                {
                    if (typeof(IEntity).IsAssignableFrom(property.PropertyType))
                    {
                        var value = (IEntity)property.GetValue(entity);
                        if (value != null)
                        {
                            otherEntities.Add(value);
                        }
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        var enumerable = (IEnumerable)property.GetValue(entity);
                        if (enumerable != null)
                        {
                            otherEntities.AddRange(enumerable.OfType<IEntity>());
                        }
                    }
                }

                if (otherEntities.Any())
                {
                    AssertValid(otherEntities, examinedEntities);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        [Trait("Test Type", "Integration Test")]
        public virtual void IntegrationTestFindById()
        {
            var container = GetIntegrationTestContainer();
            var entityIds = GetEntityIds(container);
            var repository = container.Resolve<TRepositoryInterface>();
            foreach (var entityId in entityIds)
            {
                var entity = repository.Find(entityId);
                AssertValid(entity);
                Assert.Equal(entityId, entity.EntityId.Value);
            }
        }

        [Fact]
        [Trait("Test Type", "Integration Test")]
        public virtual void IntegrationTestFindByIds()
        {
            var container = GetIntegrationTestContainer();
            var entityIds = GetEntityIds(container);
            var repository = container.Resolve<TRepositoryInterface>();
            var entities = repository.Find(entityIds);
            AssertValid(entities);
            BetterAssert.ContainsSame(entityIds, entities.Select(entity => entity.EntityId.Value).ToArray());
        }

        [Fact]
        [Trait("Test Type", "Unit Test")]
        public virtual void TestFindAll()
        {
            using (var testHelper = GetNewRepositoryTestHelper())
            {
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
        }

        [Fact]
        [Trait("Test Type", "Unit Test")]
        public virtual void TestFindById()
        {
            using (var testHelper = GetNewRepositoryTestHelper())
            {
                var domainEntity = testHelper.TestData<TEntity>().RandomItem();
                var foundEntity = testHelper.TestedObject.Find(domainEntity.EntityId.Value);
                AssertValid(foundEntity);
                Assert.Equal(domainEntity.EntityId.Value, foundEntity.EntityId);
            }
        }

        [Fact]
        [Trait("Test Type", "Unit Test")]
        public virtual void TestFindByIds()
        {
            using (var testHelper = GetNewRepositoryTestHelper())
            {
                var entityIds = testHelper.TestData<TEntity>().RandomItems(10, 10).Select(entity => entity.EntityId.Value).Distinct().ToArray();
                var foundEntities = testHelper.TestedObject.Find(entityIds);
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
        }

        protected abstract ITestHelper<TRepositoryInterface> GetNewRepositoryTestHelper();

        protected virtual Guid CreateEntityId(params object[] primaryKeys) => (Guid)primaryKeys[0];

        protected virtual string CreateQueryForPrimaryKeys() => $"SELECT {typeof(TEntity).Name}Id FROM {typeof(TEntity).Name}";

        protected virtual void Dispose(bool disposedManaged)
        {
            var testDatabasesDirectory = new DirectoryInfo(Path.GetTempPath() + "\\IntegrationTestDatabases");

            foreach (var testDatabaseDirectory in testDatabasesDirectory.EnumerateDirectories())
            {
                try
                {
                    testDatabaseDirectory.Delete(true);
                }
                catch
                {
                    // We don't care why the delete failed, we will just skip it
                }
            }
        }

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

        protected virtual IContainer GetIntegrationTestContainer()
        {
            var emptyDatabaseDataFile = new FileInfo(EmptyTestDatabaseFilePath);
            Assert.True(emptyDatabaseDataFile.Exists, $"The path for the empty database data file is not valid: '{emptyDatabaseDataFile.FullName}'");
            var instanceId = Guid.NewGuid().ToString().Replace("-", string.Empty);

            var tempDatabaseDirectory = new DirectoryInfo($"{Path.GetTempPath()}\\IntegrationTestDatabases\\{instanceId}");
            tempDatabaseDirectory.Create();
            var dataFileName = $"{tempDatabaseDirectory.FullName}\\{emptyDatabaseDataFile.Name.SubstringBeforeLast(".")}-{instanceId}.mdf";
            lock (_lockSync)
            {
                emptyDatabaseDataFile.CopyTo(dataFileName);
            }

            var container = DefaultContainer.CreateChildContainer();
            container.Resolve<IConnectionFactoryRegistry>().RegisterFactory(ConnectionName, () => new SqlConnection($"Data Source=(LocalDB)\\MSSQLLocalDB; AttachDbFilename={dataFileName}; Integrated Security=True"));

            return container;
        }
    }
}

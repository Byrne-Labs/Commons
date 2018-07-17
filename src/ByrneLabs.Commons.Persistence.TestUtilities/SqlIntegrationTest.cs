using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using ByrneLabs.Commons.Domain;
using ByrneLabs.Commons.Ioc;
using JetBrains.Annotations;
using Xunit;

namespace ByrneLabs.Commons.Persistence.TestUtilities
{
    [PublicAPI]
    public abstract class SqlIntegrationTest : IDisposable
    {
        private readonly object _lockSync = new object();

        protected abstract string ConnectionName { get; }

        protected abstract string EmptyTestDatabaseFilePath { get; }

        protected static void AssertValid(IEntity entity) => AssertValid(new[] { entity });

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
                        var value = (IEntity) property.GetValue(entity);
                        if (value != null)
                        {
                            otherEntities.Add(value);
                        }
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        var enumerable = (IEnumerable) property.GetValue(entity);
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

            var container = new SimpleContainerProvider(true);
            container.Resolve<IConnectionFactoryRegistry>().RegisterFactory(ConnectionName, () => new SqlConnection($"Data Source=(LocalDB)\\MSSQLLocalDB; AttachDbFilename={dataFileName}; Integrated Security=True"));

            return container;
        }
    }
}

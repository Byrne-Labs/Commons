using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using ByrneLabs.Commons.Ioc;
using JetBrains.Annotations;
using Xunit;

namespace ByrneLabs.Commons.Persistence.TestUtilities
{
    [PublicAPI]
    public abstract class SqlRepositoryIntegrationTest : IDisposable
    {
        private IList<FileInfo> _temporaryDatabaseFiles = new List<FileInfo>();

        protected abstract string ConnectionName { get; }

        protected abstract string EmptyTestDatabaseDataFilePath { get; }

        protected abstract string EmptyTestDatabaseLogFilePath { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposedManaged)
        {
            foreach (var temporaryDatabaseFile in _temporaryDatabaseFiles)
            {
                temporaryDatabaseFile.Delete();
            }
        }

        protected void SetupTest(IContainer container)
        {
            var emptyDatabaseDataFile = new FileInfo(EmptyTestDatabaseDataFilePath);
            var emptyDatabaseLogFile = new FileInfo(EmptyTestDatabaseLogFilePath);
            Assert.True(emptyDatabaseDataFile.Exists, $"The path for the empty database data file is not valid: '{emptyDatabaseDataFile.FullName}'");
            Assert.True(emptyDatabaseLogFile.Exists, $"The path for the empty database log file is not valid: '{emptyDatabaseLogFile.FullName}'");

            var tempDatabasePath = Path.GetTempFileName();
            var tempDatabaseDataFile = emptyDatabaseDataFile.CopyTo($"{tempDatabasePath}\\{emptyDatabaseDataFile.Name}");
            var tempDatabaseLogFile = emptyDatabaseDataFile.CopyTo($"{tempDatabasePath}\\{emptyDatabaseLogFile.Name}");
            _temporaryDatabaseFiles.Add(tempDatabaseDataFile);
            _temporaryDatabaseFiles.Add(tempDatabaseLogFile);

            container.Resolve<IConnectionFactoryRegistry>().RegisterFactory(ConnectionName, () => new SqlConnection($"Data Source=(LocalDB)\\MSSQLLocalDB; AttachDbFilename={tempDatabaseDataFile.FullName}; Integrated Security=True; User Instance=True"));
        }
    }
}

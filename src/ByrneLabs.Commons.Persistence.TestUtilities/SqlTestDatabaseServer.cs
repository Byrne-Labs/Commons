using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using ByrneLabs.Commons.Ioc;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Persistence.TestUtilities
{
    [PublicAPI]
    public sealed class SqlTestDatabaseServer : IDisposable
    {
        private readonly IList<WeakReference<SqlConnection>> _connections = new List<WeakReference<SqlConnection>>();
        private readonly string _instanceId;

        public SqlTestDatabaseServer(string emptyTestDatabaseFilePath)
        {
            _instanceId = Guid.NewGuid().ToString().Replace("-", string.Empty);
            DatabaseDirectory = new DirectoryInfo($"{Path.GetTempPath()}\\IntegrationTestDatabases\\{_instanceId}");
            DatabaseDirectory.Create();
            var emptyDatabaseDataFile = new FileInfo(emptyTestDatabaseFilePath);
            var databaseName = $"{emptyDatabaseDataFile.Name.SubstringBeforeLast(".")}-{_instanceId}";
            var dataFileName = $"{DatabaseDirectory.FullName}\\{databaseName}.mdf";
            emptyDatabaseDataFile.CopyTo(dataFileName);
            ConnectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB; AttachDbFilename={dataFileName}; Integrated Security=True";
        }

        public string ConnectionString { get; }

        public DirectoryInfo DatabaseDirectory { get; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Cleanup();
        }

        public void Register(IContainer container, string connectionName)
        {
            container.Resolve<IConnectionFactoryRegistry>().RegisterFactory(connectionName, () =>
                {
                    var connection = new SqlConnection(ConnectionString);
                    _connections.Add(new WeakReference<SqlConnection>(connection));

                    return connection;
                }
            );
        }

        private void Cleanup()
        {
            foreach (var connectionReference in _connections)
            {
                connectionReference.TryGetTarget(out var connection);
                if (connection != null)
                {
                    try
                    {
                        connection.Close();
                    }
                    catch
                    {
                        // This should fail if the connection was correctly closed but we want to try just to make sure
                    }

                    try
                    {
                        connection.Dispose();
                    }
                    catch
                    {
                        // This should fail if the connection was correctly disposed but we want to try just to make sure
                    }
                }
            }

            using (var connection = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB; Initial Catalog=master; Integrated Security=True"))
            {
                connection.Open();

                string fullDatabaseName;
                using (var command = new SqlCommand($"SELECT name FROM sys.databases WHERE name LIKE '%{_instanceId}%'"))
                {
                    command.Connection = connection;
                    fullDatabaseName = (string)command.ExecuteScalar();
                }

                using (var command = new SqlCommand($"ALTER DATABASE [{fullDatabaseName}] SET OFFLINE WITH ROLLBACK IMMEDIATE"))
                {
                    command.Connection = connection;
                    command.ExecuteNonQuery();
                }

                using (var command = new SqlCommand($"EXEC sp_detach_db '{fullDatabaseName}'"))
                {
                    command.Connection = connection;
                    command.ExecuteNonQuery();
                }
            }

            try
            {
                DatabaseDirectory.Delete(true);
            }
            catch
            {
                // We don't want the app to crash if the file can for some reason not be deleted
            }
        }

        ~SqlTestDatabaseServer()
        {
            Cleanup();
        }
    }
}

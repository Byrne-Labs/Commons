using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mime;
using ByrneLabs.Commons.Ioc;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Persistence.TestUtilities
{
    [PublicAPI]
    public sealed class SqlTestDatabaseServer : IDisposable
    {
        private readonly IList<WeakReference<SqlConnection>> _connections = new List<WeakReference<SqlConnection>>();


        ~SqlTestDatabaseServer()
        {
            Cleanup();
        }

        public SqlTestDatabaseServer(string emptyTestDatabaseFilePath)
        {
            var instanceId = Guid.NewGuid().ToString().Replace("-", string.Empty);
            DatabaseDirectory = new DirectoryInfo($"{Path.GetTempPath()}\\IntegrationTestDatabases\\{instanceId}");
            DatabaseDirectory.Create();
            var emptyDatabaseDataFile = new FileInfo(emptyTestDatabaseFilePath);
            var dataFileName = $"{DatabaseDirectory.FullName}\\{emptyDatabaseDataFile.Name.SubstringBeforeLast(".")}-{instanceId}.mdf";
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

            try
            {
                DatabaseDirectory.Delete(true);
            }
            catch
            {
                // We don't want the app to crash if the file can for some reason not be deleted
            }
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
    }
}

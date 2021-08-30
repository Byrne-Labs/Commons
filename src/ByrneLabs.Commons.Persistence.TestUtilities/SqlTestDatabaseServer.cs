using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
            _instanceId = Guid.NewGuid().ToString().Replace("-", string.Empty, StringComparison.InvariantCulture);
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

        public IDbConnection OpenConnection() => new SqlConnection(ConnectionString);

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
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        // This should fail if the connection was correctly closed but we want to try just to make sure
                    }

                    try
                    {
                        connection.Dispose();
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        // This should fail if the connection was correctly disposed but we want to try just to make sure
                    }
                }
            }

            using (var connection = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB; Initial Catalog=master; Integrated Security=True"))
            {
                connection.Open();

                string fullDatabaseName;
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                using (var command = new SqlCommand($"SELECT name FROM sys.databases WHERE name LIKE '%{_instanceId}%'"))
                {
                    command.Connection = connection;
                    fullDatabaseName = (string) command.ExecuteScalar();
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
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            }

            try
            {
                DatabaseDirectory.Delete(true);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
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

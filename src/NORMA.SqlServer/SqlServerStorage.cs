using System;
using System.Configuration;
using System.Data.SqlClient;
using Norma.Storage;

namespace Norma.SqlServer
{
    public class SqlServerStorage : AuditLogStorage
    {
        private readonly SqlConnection _existingConnection;
        private readonly SqlServerStorageOptions _options;
        private readonly string _connectionString;

        public SqlServerStorage(string nameOrConnectionString) : this(nameOrConnectionString,new SqlServerStorageOptions())
        {
        }

        public SqlServerStorage(string nameOrConnectionString, SqlServerStorageOptions options)
        {
            _options = options;
            if (nameOrConnectionString == null) throw new ArgumentNullException("nameOrConnectionString");
            if (options == null) throw new ArgumentNullException("options");

            _options = options;

            if (IsConnectionString(nameOrConnectionString))
            {
                _connectionString = nameOrConnectionString;
            }
            else if (IsConnectionStringInConfiguration(nameOrConnectionString))
            {
                _connectionString = ConfigurationManager.ConnectionStrings[nameOrConnectionString].ConnectionString;
            }
            else
            {
                throw new ArgumentException(
                    string.Format("Could not find connection string with name '{0}' in application config file",
                                  nameOrConnectionString));
            }

            if (options.PrepareSchemaIfNecessary)
            {
                using (var connection = CreateAndOpenConnection())
                {
                    SqlServerObjectsInstaller.Install(connection);
                }
            }

        }

        public SqlServerStorage(SqlConnection existingConnection)
        {
            if (existingConnection == null) throw new ArgumentNullException("existingConnection");

            _existingConnection = existingConnection;
            _options = new SqlServerStorageOptions();

        }

        internal SqlConnection CreateAndOpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();

            return connection;
        }

        private bool IsConnectionString(string nameOrConnectionString)
        {
            return nameOrConnectionString.Contains(";");
        }

        private bool IsConnectionStringInConfiguration(string connectionStringName)
        {
            var connectionStringSetting = ConfigurationManager.ConnectionStrings[connectionStringName];

            return connectionStringSetting != null;
        }


        public override IStorageConnection GetConnection()
        {
            var connection = _existingConnection ?? CreateAndOpenConnection();
            return new SqlServerConnection(connection, _existingConnection == null);
        }

        public override DateTime AuditDateTime
        {
            get { return _options.AuditDateAction(); }
        }
    }
}
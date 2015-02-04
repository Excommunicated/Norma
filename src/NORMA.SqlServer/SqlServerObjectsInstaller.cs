using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Dapper;
using Norma.Logging;

namespace Norma.SqlServer
{
    internal static class SqlServerObjectsInstaller
    {
        private const int RequiredSchemaVersion = 1;
        private static readonly ILog Log = LogProvider.GetLogger(typeof(SqlServerObjectsInstaller));

        public static void Install(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            Log.Info("Start installing Norma SQL objects...");

            if (!IsSqlEditionSupported(connection))
            {
                throw new PlatformNotSupportedException("The SQL Server edition of the target server is unsupported, e.g. SQL Azure.");
            }

            var script = GetStringResource(
                typeof(SqlServerObjectsInstaller).Assembly,
                "Norma.SqlServer.Install.sql");

            script = script.Replace("SET @TARGET_SCHEMA_VERSION = 1;", "SET @TARGET_SCHEMA_VERSION = " + RequiredSchemaVersion + ";");

            connection.Execute(script);

            Log.Info("Norma SQL objects installed.");
        }

        private static string GetStringResource(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException(String.Format(
                        "Requested resource `{0}` was not found in the assembly `{1}`.",
                        resourceName,
                        assembly));
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        private static bool IsSqlEditionSupported(SqlConnection connection)
        {
            var edition = connection.Query<int>("SELECT SERVERPROPERTY ( 'EngineEdition' )").Single();
            return edition >= SqlEngineEdition.Standard && edition <= SqlEngineEdition.SqlAzure;
        }
        private static class SqlEngineEdition
        {
            public const int Personal = 1;
            public const int Standard = 2;
            public const int Enterprise = 3;
            public const int Express = 4;
            public const int SqlAzure = 5;
        }
    }
}
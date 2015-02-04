using System;

namespace Norma.SqlServer
{
    public class SqlServerStorageOptions
    {
        public SqlServerStorageOptions()
        {
            PrepareSchemaIfNecessary = true;
            AuditDateAction = () => DateTime.UtcNow;
        }

        public bool PrepareSchemaIfNecessary { get; set; }
        public Func<DateTime> AuditDateAction { get; set; }
    }
}
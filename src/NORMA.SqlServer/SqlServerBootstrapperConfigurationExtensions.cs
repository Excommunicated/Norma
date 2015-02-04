namespace Norma.SqlServer
{
    public static class SqlServerBootstrapperConfigurationExtensions
    {
        public static SqlServerStorage UseSqlServerStorage(this IBootstrapperConfiguration configuration, string nameOrConnectionString)
        {
            SqlServerStorage sqlServerStorage = new SqlServerStorage(nameOrConnectionString);
            configuration.UseStorage((AuditLogStorage)sqlServerStorage);
            return sqlServerStorage;
        }

        public static SqlServerStorage UseSqlServerStorage(this IBootstrapperConfiguration configuration, string nameOrConnectionString, SqlServerStorageOptions options)
        {
            SqlServerStorage sqlServerStorage = new SqlServerStorage(nameOrConnectionString, options);
            configuration.UseStorage((AuditLogStorage)sqlServerStorage);
            return sqlServerStorage;
        }
    }
}
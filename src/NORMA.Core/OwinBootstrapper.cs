using System;
using Owin;

namespace Norma
{
    public static class OwinBootstrapper
    {
        public static void UseNorma(this IAppBuilder app, Action<IBootstrapperConfiguration> configurationAction)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (configurationAction == null) throw new ArgumentNullException("configurationAction");

            var configuration = new BootStrapperConfiguration();
            configurationAction(configuration);

            if (configuration.Activator != null)
            {
                AuditLogActivator.Current = configuration.Activator;
            }

            if (configuration.Storage == null)
            {
                throw new InvalidOperationException("Audit Log storage was not configured. Please call either `UseStorage` method or its overloads.");
            }

            AuditLogStorage.Current = configuration.Storage;

            if (configuration.OrmType == null)
            {
                throw new InvalidOperationException("Orm Type was not configured. Please call either `UseOrm` method or its overloads.");
            }

            AuditLogOrmType.Current = configuration.OrmType;
        }
    }
}
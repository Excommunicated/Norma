using System;
using System.Data.Entity.Infrastructure.Interception;
using Norma.EF.Interceptors;

namespace Norma.EF
{
    public static class EFBootstrapperConfigurationExtensions
    {
        public static AuditLogOrmType UseEntityFrameworkDbInterceptor(
            this IBootstrapperConfiguration configuration, Action<AuditableEntityModelBuilder> propertyMappings = null)
        {
            var ormType = new EntityFrameworkOrmType();
            DbInterception.Add(new EFAuditLogInterceptor());
            var mappingsConfig = new AuditableEntityModelBuilder();
            if (propertyMappings != null)
            {
                propertyMappings(mappingsConfig);
                ormType.AuditableEntityModelConfiguration = mappingsConfig.ModelConfiguration;
            }
            configuration.UseOrm(ormType);
            return ormType;
        }
    }
    public class EntityFrameworkOrmType : AuditLogOrmType
    {
        public override AuditableEntityModelConfiguration AuditableEntityModelConfiguration { get; set; }
    }
}
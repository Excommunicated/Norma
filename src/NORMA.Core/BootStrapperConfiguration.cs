namespace Norma
{
    public class BootStrapperConfiguration : IBootstrapperConfiguration
    {
        public AuditLogStorage Storage { get; private set; }
        public AuditLogOrmType OrmType { get; private set; }
        public AuditLogActivator Activator { get; private set; }
        
        public void UseStorage(AuditLogStorage storage)
        {
            Storage = storage;
        }

        public void UseOrm(AuditLogOrmType ormType)
        {
            OrmType = ormType;
        }

        public void UseActivator(AuditLogActivator activator)
        {
            Activator = activator;
        }
    }
}
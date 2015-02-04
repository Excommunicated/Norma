namespace Norma
{
    public interface IBootstrapperConfiguration
    {
        void UseStorage(AuditLogStorage storage);
        void UseOrm(AuditLogOrmType ormType);
        void UseActivator(AuditLogActivator activator);
    }
}
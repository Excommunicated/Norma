using System;
using Norma.Model;

namespace Norma.Storage
{
    public interface IStorageConnection : IDisposable
    {
        IWriteOnlyTransaction CreateWriteOnlyTransaction();
        Guid AddAuditLog(string entityId, string entityFullName, string user, DateTime auditDateTime);
    }

    public interface IWriteOnlyTransaction : IDisposable
    {
        void AddToAuditLogChange(AuditLogChange change);

        void Commit();
    }
}
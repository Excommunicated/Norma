using System;

namespace Norma.Model
{
    [Serializable]
    public class AuditLog
    {
        public AuditLog()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedUser { get; set; }
        public string EntityFullName { get; set; }
        public string EntityId { get; set; }
        
    }

    public class AuditLogChange
    {
        public AuditLogChange()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public Guid AuditLogId { get; set; }
        public DateTime Created { get; set; }
        public string User { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string PropertyName { get; set; }
        public LogOperation Operation { get; set; }
    }

    public class ChangedProperty
    {
        public string Name;
        public object CurrentValue;
        public object OriginalValue;
    }

    public enum LogOperation
    {
        Create,
        Update,
        Delete,
        Unchanged
    }
}
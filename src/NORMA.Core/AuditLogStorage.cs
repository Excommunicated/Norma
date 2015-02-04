using System;
using Norma.Storage;

namespace Norma
{
    public abstract class AuditLogStorage
    {
        private static readonly object LockObject = new object();
        private static AuditLogStorage _current;

        public static AuditLogStorage Current
        {
            get
            {
                lock (LockObject)
                {
                    if (_current == null)
                    {
                        throw new InvalidOperationException("AuditLogStorage.Current property value has not been initialized. You must set it before using Norma");
                    }
                    return _current;
                }
            }
            set
            {
                lock (LockObject)
                {
                    _current = value;
                }
            }
        }

        public abstract IStorageConnection GetConnection();

        public virtual DateTime AuditDateTime { get { return DateTime.Now; } }
    }
}

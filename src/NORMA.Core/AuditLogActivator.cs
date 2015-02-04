using System;

namespace Norma
{
    public class AuditLogActivator
    {
        private static AuditLogActivator _current = new AuditLogActivator();

        public static AuditLogActivator Current
        {
            get { return _current; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _current = value;
            }
        }

        public virtual object Activate(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}
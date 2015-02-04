using System;

namespace Norma.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class AuditableAttribute : Attribute
    {
    }
}

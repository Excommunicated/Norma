using System;

namespace Norma.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class NotAuditableAttribute : Attribute
    {
    }
}
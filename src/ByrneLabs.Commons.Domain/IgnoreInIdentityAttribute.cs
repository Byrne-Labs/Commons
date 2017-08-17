using System;

namespace ByrneLabs.Commons.Domain
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreInIdentityAttribute : Attribute
    {
    }
}

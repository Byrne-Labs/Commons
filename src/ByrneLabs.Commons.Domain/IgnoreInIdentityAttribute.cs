using System;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Domain
{
    [AttributeUsage(AttributeTargets.Property)]
    [PublicAPI]
    public sealed class IgnoreInIdentityAttribute : Attribute
    {
    }
}

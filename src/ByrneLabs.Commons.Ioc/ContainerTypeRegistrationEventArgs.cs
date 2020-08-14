using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Ioc
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "These are likely to be used in the future")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "These are likely to be used in the future")]
    [PublicAPI]
    public class ContainerTypeRegistrationEventArgs : EventArgs
    {
        public ContainerTypeRegistrationEventArgs(IContainer container, Type from, Type to, string name)
        {
            Container = container;
            From = from;
            To = to;
            Name = name;
        }

        public IContainer Container { get; }

        public Type From { get; }

        public string Name { get; }

        public Type To { get; }
    }
}

using System;
using System.Diagnostics.CodeAnalysis;
using ByrneLabs.Commons.Ioc;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.TestUtilities
{
    [PublicAPI]
    public interface ITestHelper : ITestDataProvider, IDisposable
    {
        IContainer Container { get; }
    }

    [PublicAPI]
    public interface ITestHelper<out TInterface> : ITestHelper where TInterface : class
    {
        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global", Justification = "This is something likely to be used in the future")]
        TInterface TestedObject { get; }
    }
}

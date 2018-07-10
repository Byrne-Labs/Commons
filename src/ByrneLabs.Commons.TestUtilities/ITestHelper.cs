using System;
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
        TInterface TestedObject { get; }
    }
}

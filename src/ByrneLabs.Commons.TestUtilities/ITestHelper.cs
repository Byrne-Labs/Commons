using System;
using System.Diagnostics.CodeAnalysis;

namespace ByrneLabs.Commons.TestUtilities
{
    public interface ITestHelper : ITestDataProvider, IDisposable
    {
        IServiceProvider ServiceProvider { get; }
    }

    public interface ITestHelper<out TInterface> : ITestHelper where TInterface : class
    {
        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global", Justification = "This is something likely to be used in the future")]
        TInterface TestedObject { get; }
    }
}

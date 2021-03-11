using System;

namespace ByrneLabs.Commons.Locks
{
    public interface ILock : IDisposable
    {
        bool IsAcquired { get; }

        string Resource { get; }
    }
}

using System;

namespace ByrneLabs.Commons.Locks
{
    public interface ILockFactory
    {
        ILock CreateLock(string resource, TimeSpan expiryTime, TimeSpan waitTime, TimeSpan retryTime);
    }
}

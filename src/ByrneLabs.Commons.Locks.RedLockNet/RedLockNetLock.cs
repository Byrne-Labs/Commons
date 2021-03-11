using RedLockNet;

namespace ByrneLabs.Commons.Locks.RedLockNet
{
    public class RedLockNetLock : ILock
    {
        private readonly IRedLock _redLock;

        public RedLockNetLock(IRedLock redLock)
        {
            _redLock = redLock;
        }

        public bool IsAcquired => _redLock.IsAcquired;

        public string Resource => _redLock.Resource;

        public void Dispose()
        {
            _redLock.Dispose();
        }
    }
}

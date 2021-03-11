using System;
using System.Threading;

namespace ByrneLabs.Commons.Locks
{
    public class MutexLock : ILock
    {
        private readonly Action<string> _mutexReleased;

        public MutexLock(string resource, Mutex mutex, Action<string> mutexReleased)
        {
            Resource = resource;
            Mutex = mutex;
            _mutexReleased = mutexReleased;
        }

        public bool IsAcquired => true;

        public string Resource { get; }

        internal Mutex Mutex { get; }

        public void Dispose()
        {
            Mutex.ReleaseMutex();
            _mutexReleased.Invoke(Resource);
        }
    }
}

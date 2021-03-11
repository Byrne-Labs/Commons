using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Locks
{
    [PublicAPI]
    public class MutexLockFactory : ILockFactory
    {
        private readonly Dictionary<string, MutexLock> _mutexes = new Dictionary<string, MutexLock>();

        public ILock CreateLock(string resource, TimeSpan expiryTime, TimeSpan waitTime, TimeSpan retryTime)
        {
            MutexLock mutexLock;
            lock (_mutexes)
            {
                if (!_mutexes.ContainsKey(resource))
                {
                    var mutex = new Mutex(true, resource);
                    mutexLock = new MutexLock(resource, mutex, MutexReleased);
                    _mutexes.Add(resource, mutexLock);
                }
                else
                {
                    mutexLock = _mutexes[resource];
                }
            }

            mutexLock.Mutex.WaitOne(waitTime);

            return mutexLock;
        }

        private void MutexReleased(string resource)
        {
            lock (_mutexes)
            {
                if (_mutexes.ContainsKey(resource))
                {
                    _mutexes.Remove(resource);
                }
            }
        }
    }
}

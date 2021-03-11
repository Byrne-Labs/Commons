using System;
using System.Collections.Generic;
using System.Linq;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;

namespace ByrneLabs.Commons.Locks.RedLockNet
{
    public class RedLockNetLockFactory : ILockFactory
    {
        private readonly RedLockFactory _redLockFactory;

        public RedLockNetLockFactory(IEnumerable<RedLockEndPoint> endPoints)
        {
            _redLockFactory = RedLockFactory.Create(endPoints.ToList());
        }

        public ILock CreateLock(string resource, TimeSpan expiryTime, TimeSpan waitTime, TimeSpan retryTime)
        {
            var redLock = _redLockFactory.CreateLock(resource, expiryTime, waitTime, retryTime);

            return new RedLockNetLock(redLock);
        }
    }
}

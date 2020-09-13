using System;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Persistence.Dapper
{
    [PublicAPI]
    public class LookupBridge
    {
        public Guid ConsumerId { get; set; }

        public Guid LookupBridgeId { get; set; }

        public Guid LookupId { get; set; }
    }
}

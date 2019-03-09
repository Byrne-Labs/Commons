using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.MasterDataManagement
{
    [PublicAPI]
    public class RecordMetadata
    {
        public IDictionary<DataSource, int> DefaultPriority { get; } = new ConcurrentDictionary<DataSource, int>();

        public List<MemberMetadata> MemberMetadata { get; } = new List<MemberMetadata>();

        public Type RecordType { get; set; }

        public Func<Guid> GetRecordIdentifier { get; set; }
    }
}

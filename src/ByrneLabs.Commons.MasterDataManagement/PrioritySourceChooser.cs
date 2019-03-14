using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.MasterDataManagement
{
    [PublicAPI]
    public class PrioritySourceChooser : ISourceChooser
    {
        public int Priority { get; set; }

        public bool CanChooseSource(IEnumerable<SourceRecord> sourceRecords, RecordMetadata recordMetadata, PropertyInfo propertyInfo) => true;

        public SourceRecord ChooseSource(IEnumerable<SourceRecord> sourceRecords, RecordMetadata recordMetadata, PropertyInfo propertyInfo)
        {
        }
    }
}

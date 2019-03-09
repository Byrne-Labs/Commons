using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.MasterDataManagement
{
    [PublicAPI]
    public interface ISourceChooser
    {
        int Priority { get; set; }

        bool CanChooseSource(IEnumerable<SourceRecord> sourceRecords, RecordMetadata recordMetadata, PropertyInfo propertyInfo);

        SourceRecord ChooseSource(IEnumerable<SourceRecord> sourceRecords, RecordMetadata recordMetadata, PropertyInfo propertyInfo);
    }
}

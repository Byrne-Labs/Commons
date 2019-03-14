using System;
using System.Collections.Generic;
using System.Reflection;

namespace ByrneLabs.Commons.MasterDataManagement
{
    public abstract class CalculatedSourceChooser : ISourceChooser
    {
        public int Priority { get; set; }

        public bool CanChooseSource(IEnumerable<SourceRecord> sourceRecords, RecordMetadata recordMetadata, PropertyInfo propertyInfo) => throw new NotImplementedException();

        public SourceRecord ChooseSource(IEnumerable<SourceRecord> sourceRecords, RecordMetadata recordMetadata, PropertyInfo propertyInfo) => throw new NotImplementedException();
    }
}

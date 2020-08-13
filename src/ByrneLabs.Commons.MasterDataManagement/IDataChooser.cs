using System.Collections.Generic;
using System.Reflection;

namespace ByrneLabs.Commons.MasterDataManagement
{
    public interface IDataChooser<T>
    {
        bool CanChooseData(IEnumerable<SourceRecord<T>> sourceRecords, RecordMetaData recordMetaData, FieldInfo fieldInfo);

        bool CanChooseData(IEnumerable<SourceRecord<T>> sourceRecords, RecordMetaData recordMetaData, PropertyInfo propertyInfo);

        object ChooseData(IEnumerable<SourceRecord<T>> sourceRecords, RecordMetaData recordMetaData, FieldInfo fieldInfo);

        object ChooseData(IEnumerable<SourceRecord<T>> sourceRecords, RecordMetaData recordMetaData, PropertyInfo propertyInfo);
    }
}

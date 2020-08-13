using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ByrneLabs.Commons.MasterDataManagement
{
    public class QualityDataChooser:IDataChooser
    {
        public object ChooseData(IEnumerable<(object, DataSource)> data, RecordMetaData recordMetaData, FieldInfo fieldInfo) => throw new NotImplementedException();

        public object ChooseData(IEnumerable<(object, DataSource)> data, RecordMetaData recordMetaData, PropertyInfo propertyInfo) => throw new NotImplementedException();
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using ByrneLabs.Commons.Domain;

namespace ByrneLabs.Commons.MasterDataManagement
{
    public class CompositeObject<T> where T : new()
    {
        private readonly T _masterRecord;
        private List<(T, DataSource)> _sourceRecords;
        private IDictionary<FieldInfo, (DataSource, T)> _fieldValues;
        private IDictionary<PropertyInfo, (DataSource, T)> _propertyValues;
        private IEnumerable<IDataChooser> _dataChoosers;

        public CompositeObject(List<(T, DataSource)> sourceRecords, IEnumerable<IDataChooser> dataChoosers)
        {
            _masterRecord = new T();
            _dataChoosers = dataChoosers;
        }

        public object this[string name, DataSource dataSource]
        {
            set
            {

            }
        }

        public object this[FieldInfo fieldInfo, DataSource dataSource]
        {
            set
            {

            }
        }

        public object this[PropertyInfo propertyInfo, DataSource dataSource]
        {
            set
            {

            }
        }

        public (object, DataSource) this[string name]
        {
            get
            {

            }
        }

        public (object, DataSource) this[FieldInfo fieldInfo]
        {
            get
            {

            }
        }

        public (object, DataSource) this[PropertyInfo propertyInfo]
        {
            get
            {

            }
        }
    }
}

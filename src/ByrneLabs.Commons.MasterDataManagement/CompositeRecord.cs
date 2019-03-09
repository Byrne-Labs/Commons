using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.MasterDataManagement
{
    [PublicAPI]
    public class CompositeRecord
    {
        private readonly IDictionary<PropertyInfo, SourceRecord> _manuallyChosenPropertySources;
        private readonly RecordMetadata _recordMetadata;
        private readonly Type _recordType;
        private readonly IEnumerable<ISourceChooser> _sourceChoosers;
        private readonly IList<SourceRecord> _sourceRecords;
        private IDictionary<PropertyInfo, SourceRecord> _propertySources;

        internal CompositeRecord(Type recordType, IEnumerable<SourceRecord> sourceRecords, RecordMetadata recordMetadata, IEnumerable<ISourceChooser> dataChoosers, IDictionary<PropertyInfo, SourceRecord> manuallyChosenPropertySources)
        {
            _recordType = recordType;
            _sourceRecords = sourceRecords.ToList();
            _recordMetadata = recordMetadata;
            _sourceChoosers = dataChoosers.OrderBy(dataChooser => dataChooser.Priority).ToList();
            _manuallyChosenPropertySources = manuallyChosenPropertySources;
            foreach (var sourceRecord in _sourceRecords.Select(s => s.Data).OfType<INotifyPropertyChanged>())
            {
                sourceRecord.PropertyChanged += (sender, args) => CalculateMasterRecord();
            }

            CalculateMasterRecord();
        }

        public object MasterRecord { get; private set; }

        public void AddManuallyChosenPropertySource(PropertyInfo propertyInfo, SourceRecord sourceRecord)
        {
            if (!_sourceRecords.Contains(sourceRecord))
            {
                throw new ArgumentException("The source record was not found on this composite record.", nameof(sourceRecord));
            }

            _manuallyChosenPropertySources[propertyInfo] = sourceRecord;
            CalculateMasterRecord();
        }

        public void CalculateMasterRecord()
        {
            MasterRecord = _recordType.GetConstructor(Array.Empty<Type>()).Invoke(null);
            _propertySources = new Dictionary<PropertyInfo, SourceRecord>();
            foreach (var propertyInfo in _recordType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.CanRead && p.CanWrite))
            {
                ChooseProperty(propertyInfo);
            }
        }

        public (V, DataSource) GetPropertyValue<V>(string propertyName)
        {
            var (value, dataSource) = GetPropertyValue(propertyName);
            return ((V) value, dataSource);
        }

        public (V, DataSource) GetPropertyValue<V>(PropertyInfo propertyInfo)
        {
            var (value, dataSource) = GetPropertyValue(propertyInfo);
            return ((V) value, dataSource);
        }

        public (object, DataSource) GetPropertyValue(string propertyName) => GetPropertyValue(_recordType.GetProperty(propertyName));

        public (object, DataSource) GetPropertyValue(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.IsDefined(_recordType))
            {
                throw new ArgumentException($"The property {propertyInfo.Name} is declared on type {propertyInfo.DeclaringType.FullName} is not valid for type {_recordType.FullName}");
            }

            if (!_propertySources.ContainsKey(propertyInfo))
            {
                throw new ArgumentException($"No value could be calculated for {propertyInfo.Name}", nameof(propertyInfo));
            }

            return (propertyInfo.GetValue(MasterRecord), _propertySources[propertyInfo].DataSource);
        }

        private void ChooseProperty(PropertyInfo propertyInfo)
        {
            if (_manuallyChosenPropertySources.ContainsKey(propertyInfo))
            {
                _propertySources.Add(propertyInfo, _manuallyChosenPropertySources[propertyInfo]);
            }
            else
            {
                foreach (var dataSource in from sourceChooser in _sourceChoosers where sourceChooser.CanChooseSource(_sourceRecords, _recordMetadata, propertyInfo) select sourceChooser.ChooseSource(_sourceRecords, _recordMetadata, propertyInfo))
                {
                    _propertySources.Add(propertyInfo, dataSource);
                    break;
                }
            }

            if (_propertySources.ContainsKey(propertyInfo))
            {
                propertyInfo.SetValue(MasterRecord, _propertySources[propertyInfo].Data);
            }
        }
    }
}

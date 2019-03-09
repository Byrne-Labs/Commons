using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.MasterDataManagement
{
    [PublicAPI]
    public class DataManager
    {
        private readonly IEnumerable<ISourceChooser> _dataChoosers;
        private readonly IDictionary<Type, RecordMetadata> _recordMetadata;

        public DataManager(IEnumerable<RecordMetadata> recordMetadata, IEnumerable<ISourceChooser> dataChoosers)
        {
            foreach (var recordMetaDatum in recordMetadata.Where(recordMetaDatum1 => recordMetadata.Any(recordMetaDatum2 => recordMetaDatum2 != recordMetaDatum1 && recordMetaDatum2.RecordType == recordMetaDatum1.RecordType)))
            {
                throw new ArgumentException($"The record type {recordMetaDatum.RecordType.FullName} has more than one record metadata object.", nameof(recordMetadata));
            }

            _recordMetadata = recordMetadata.ToDictionary(recordMetadatum => recordMetadatum.RecordType);
            _dataChoosers = dataChoosers;
        }

        public CompositeRecord CreateCompositeRecord(Type recordType, IEnumerable<SourceRecord> sourceRecords, IDictionary<PropertyInfo, SourceRecord> manuallyChosenPropertySources)
        {
            foreach (var sourceRecord in sourceRecords)
            {
                if (sourceRecords.Any(sourceRecord2 => sourceRecord2 != sourceRecord && ReferenceEquals(sourceRecord2.Data, sourceRecord.Data)))
                {
                    throw new ArgumentException("The same source record data object is used on multiple source records.", nameof(sourceRecords));
                }

                if (!sourceRecord.Data.GetType().IsInstanceOfType(recordType))
                {
                    throw new ArgumentException($"A source record data object is of type {sourceRecord.Data.GetType().FullName} which cannot be cast as {recordType.FullName}.", nameof(sourceRecords));
                }
            }

            if (!_recordMetadata.ContainsKey(recordType))
            {
                throw new ArgumentException($"There is no record metadata for type {recordType.FullName}.", nameof(sourceRecords));
            }

            return new CompositeRecord(recordType, sourceRecords, _recordMetadata[recordType], _dataChoosers, manuallyChosenPropertySources);
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.MasterDataManagement
{
    [PublicAPI]
    public class MemberMetadata
    {
        private MemberInfo _memberInfo;

        public IDictionary<DataSource, int> DefaultPriority { get; } = new ConcurrentDictionary<DataSource, int>();

        public MemberInfo MemberInfo
        {
            get => _memberInfo;
            set
            {
                switch (value)
                {
                    case PropertyInfo propertyInfo:
                    {
                        if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
                        {
                            throw new ArgumentException($"Property {value.DeclaringType.FullName}.{value.Name} must be readable and writable.");
                        }

                        break;
                    }
                    case FieldInfo fieldInfo:
                    {
                        if (fieldInfo.IsInitOnly)
                        {
                            throw new ArgumentException($"Field {value.DeclaringType.FullName}.{value.Name} must not be readonly.");
                        }

                        break;
                    }
                    default:
                        throw new ArgumentException($"Member {value.DeclaringType.FullName}.{value.Name} must be either a property or field.");
                }

                _memberInfo = value;
            }
        }

        public bool UseNullValue { get; set; }
    }
}

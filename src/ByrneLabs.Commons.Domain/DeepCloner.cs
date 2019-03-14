using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace ByrneLabs.Commons.Domain
{
    public class DeepCloner
    {
        private readonly Dictionary<long, object> _clonedObjects = new Dictionary<long, object>();
        private readonly ObjectIDGenerator _objectIdGenerator = new ObjectIDGenerator();

        private DeepCloner()
        {
        }

        public static T Clone<T>(T obj) => (T) new DeepCloner().Clone((object) obj);

        private object Clone(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var type = obj.GetType();
            if (type.IsValueType || obj is string)
            {
                return obj;
            }

            var objectId = _objectIdGenerator.GetId(obj, out var firstTime);
            if (!firstTime)
            {
                return _clonedObjects[objectId];
            }

            object clone;

            if (obj is Array array)
            {
                var elementType = type.GetElementType();
                var clonedArray = Array.CreateInstance(elementType, array.Length);
                _clonedObjects.Add(objectId, clonedArray);
                for (var index = 0; index < array.Length; index++)
                {
                    var element = array.GetValue(index);
                    var clonedElement = Clone(element);
                    clonedArray.SetValue(clonedElement, index);
                }

                clone = clonedArray;
            }
            else
            {
                clone = FormatterServices.GetUninitializedObject(type);
                _clonedObjects.Add(objectId, clone);

                var fields = new List<FieldInfo>();
                var baseType = type;
                while (baseType != null)
                {
                    fields.AddRange(baseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly).Where(f => !f.IsNotSerialized));

                    baseType = baseType.BaseType;
                }

                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(obj);
                    var clonedFieldValue = Clone(fieldValue);
                    field.SetValue(clone, clonedFieldValue);
                }
            }

            return clone;
        }
    }
}

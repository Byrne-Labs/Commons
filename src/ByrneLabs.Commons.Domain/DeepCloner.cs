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
        private readonly IDictionary<Type, IEnumerable<FieldInfo>> _fields = new Dictionary<Type, IEnumerable<FieldInfo>>();

        private DeepCloner()
        {
        }

        public static T Clone<T>(T obj) => (T)new DeepCloner().Clone(obj, obj?.GetType());

        public static TInto CloneInto<TInto, TBase>(TBase obj) where TInto : TBase => (TInto)new DeepCloner().Clone(obj, typeof(TInto));

        public static object CloneInto(object obj, Type cloneIntoType) => new DeepCloner().Clone(obj, cloneIntoType);

        private object Clone(object obj, Type cloneIntoType)
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

            if (!type.IsAssignableFrom(cloneIntoType))
            {
                throw new ArgumentException($"Type {type.FullName} is not assignable from {cloneIntoType.FullName}");
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
                    var clonedElement = Clone(element, element?.GetType());
                    clonedArray.SetValue(clonedElement, index);
                }

                clone = clonedArray;
            }
            else
            {
                clone = FormatterServices.GetUninitializedObject(cloneIntoType);
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
                    var clonedFieldValue = Clone(fieldValue, fieldValue?.GetType());
                    field.SetValue(clone, clonedFieldValue);
                }
            }

            return clone;
        }

        private IEnumerable<FieldInfo> GetFields(Type type)
        {
            if (!_fields.ContainsKey(type))
            {
                var fields = new List<FieldInfo>();
                var baseType = type;
                while (baseType != null)
                {
                    fields.AddRange(baseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly).Where(f => !f.IsNotSerialized));

                    baseType = baseType.BaseType;
                }

                _fields.Add(type, fields);
            }

            return _fields[type];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace ByrneLabs.Commons
{
    public class HandyObjectReflectionEquivalencyComparer : IEqualityComparer<HandyObject>
    {
        private readonly IList<(long, long)> _comparedObjects = new List<(long, long)>();
        private readonly IDictionary<Type, IEnumerable<FieldInfo>> _fields = new Dictionary<Type, IEnumerable<FieldInfo>>();
        private readonly ObjectIDGenerator _objectIdGenerator = new ObjectIDGenerator();

        public bool Equals(HandyObject x, HandyObject y)
        {
            var entityAObjectId = _objectIdGenerator.GetId(x, out _);
            var entityBObjectId = _objectIdGenerator.GetId(y, out _);
            if (ReferenceEquals(x, y) || x == null && y == null || _comparedObjects.Contains((entityAObjectId, entityBObjectId)))
            {
                return true;
            }

            if (x == null || y == null || x.GetType() != y.GetType())
            {
                return false;
            }

            _comparedObjects.Add((entityAObjectId, entityBObjectId));
            _comparedObjects.Add((entityBObjectId, entityAObjectId));

            foreach (var field in GetFields(x.GetType()))
            {
                var fieldValueX = field.GetValue(x);
                var fieldValueY = field.GetValue(y);
                if (x is HandyObject handyObjectX && y is HandyObject handyObjectY)
                {
                    if (!Equals(handyObjectX, handyObjectY))
                    {
                        return false;
                    }
                }
                else if (fieldValueX is IEnumerable<object> enumerableX && fieldValueY is IEnumerable<object> enumerableY)
                {
                    if (enumerableX.Count() != enumerableY.Count())
                    {
                        return false;
                    }

                    var enumeratorX = enumerableX.GetEnumerator();
                    var enumeratorY = enumerableY.GetEnumerator();
                    while (enumeratorX.MoveNext())
                    {
                        enumeratorY.MoveNext();
                        if (enumeratorX.Current is HandyObject childHandyObjectX && enumeratorY.Current is HandyObject childHandyObjectY)
                        {
                            if (!Equals(childHandyObjectX, childHandyObjectY))
                            {
                                return false;
                            }
                        }
                        else if (!Equals(enumeratorX.Current, enumeratorY.Current))
                        {
                            return false;
                        }
                    }
                }
                else if (!Equals(fieldValueX, fieldValueY))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(HandyObject obj) => obj?.GetHashCode() ?? 0;

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

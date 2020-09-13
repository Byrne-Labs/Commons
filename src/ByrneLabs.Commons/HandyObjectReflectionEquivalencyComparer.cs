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

        public bool Equals(HandyObject x, HandyObject y) => Equals1(x, y);

        public int GetHashCode(HandyObject obj) => obj?.GetHashCode() ?? 0;

        private bool Equals1(HandyObject x, HandyObject y)
        {
            if (x == null || y == null || x.GetType() != y.GetType())
            {
                return false;
            }

            var xObjectId = _objectIdGenerator.GetId(x, out _);
            var yObjectId = _objectIdGenerator.GetId(y, out _);
            if (ReferenceEquals(x, y) || x == null && y == null || _comparedObjects.Contains((xObjectId, yObjectId)))
            {
                return true;
            }

            _comparedObjects.Add((xObjectId, yObjectId));
            _comparedObjects.Add((yObjectId, xObjectId));

            foreach (var field in GetFields(x.GetType()))
            {
                var fieldValueX = field.GetValue(x);
                var fieldValueY = field.GetValue(y);

                if (fieldValueX is IEnumerable<object> enumerableX && fieldValueY is IEnumerable<object> enumerableY)
                {
                    var arrayX = enumerableX as object[] ?? enumerableX.ToArray();
                    var arrayY = enumerableY as object[] ?? enumerableY.ToArray();
                    if (arrayX.Length != arrayY.Length)
                    {
                        return false;
                    }

                    var enumeratorX = arrayX.GetEnumerator();
                    var enumeratorY = arrayY.GetEnumerator();
                    while (enumeratorX.MoveNext())
                    {
                        enumeratorY.MoveNext();
                        if (enumeratorX.Current is HandyObject childHandyObjectX && enumeratorY.Current is HandyObject childHandyObjectY)
                        {
                            if (!Equals1(childHandyObjectX, childHandyObjectY))
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
                else if (fieldValueX is HandyObject fieldValueXHandyObject && fieldValueY is HandyObject fieldValueYHandyObject)
                {
                    if (!Equals1(fieldValueXHandyObject, fieldValueYHandyObject))
                    {
                        return false;
                    }
                }
                else if (!Equals(fieldValueX, fieldValueY))
                {
                    return false;
                }
            }

            return true;
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

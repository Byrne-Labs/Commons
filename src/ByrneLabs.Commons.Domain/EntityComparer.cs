using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace ByrneLabs.Commons.Domain
{
    public class EntityComparer : IEqualityComparer<IEntity>
    {
        private readonly IList<(long, long)> _comparedObjects = new List<(long, long)>();
        private readonly IDictionary<Type, IEnumerable<FieldInfo>> _fields = new Dictionary<Type, IEnumerable<FieldInfo>>();
        private readonly ObjectIDGenerator _objectIdGenerator = new ObjectIDGenerator();

        public bool Equals(IEntity x, IEntity y)
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
                var fieldValueA = field.GetValue(x);
                var fieldValueB = field.GetValue(y);
                if (fieldValueA is IEntity fieldEntityA && fieldValueB is IEntity fieldEntityB)
                {
                    if (!Equals(fieldEntityA, fieldEntityB))
                    {
                        return false;
                    }
                }
                else if (fieldValueA is IEnumerable<IEntity> enumerableA && fieldValueB is IEnumerable<IEntity> enumerableB)
                {
                    if (!enumerableA.SequenceEqual(enumerableB, this))
                    {
                        return false;
                    }
                }
                else if (!Equals(fieldValueA, fieldValueB))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(IEntity obj) => obj?.GetType().GetHashCode() ?? 0;

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace ByrneLabs.Commons.Domain
{
    public class EntityEquivalencyComparer : IEqualityComparer<IEntity>
    {
        private readonly IList<(long, long)> _comparedObjects = new List<(long, long)>();
        private readonly IDictionary<Type, IEnumerable<FieldInfo>> _fields = new Dictionary<Type, IEnumerable<FieldInfo>>();
        private readonly ObjectIDGenerator _objectIdGenerator = new ObjectIDGenerator();

        public bool Equals(IEntity entityA, IEntity entityB)
        {
            var entityAObjectId = _objectIdGenerator.GetId(entityA, out _);
            var entityBObjectId = _objectIdGenerator.GetId(entityB, out _);
            if (ReferenceEquals(entityA, entityB) || entityA == null && entityB == null || _comparedObjects.Contains((entityAObjectId, entityBObjectId)))
            {
                return true;
            }

            if (entityA == null || entityB == null || entityA.GetType() != entityB.GetType())
            {
                return false;
            }

            _comparedObjects.Add((entityAObjectId, entityBObjectId));
            _comparedObjects.Add((entityBObjectId, entityAObjectId));

            foreach (var field in GetFields(entityA.GetType()))
            {
                var fieldValueA = field.GetValue(entityA);
                var fieldValueB = field.GetValue(entityB);
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.Domain
{
    [PublicAPI]
    public abstract class Entity : IEntity
    {
        protected Entity()
        {
            NeverPersisted = true;
        }

        public Guid? EntityId { get; set; }

        public bool HasChanged { get; set; }

        public bool NeverPersisted { get; set; }

        private static bool ReflectionEquals(IEnumerable<object> enumerableA, IEnumerable<object> enumerableB, ICollection<Tuple<Entity, Entity>> comparedEntities, int recursionLevels = 0)
        {
            if (enumerableA.Count() != enumerableB.Count())
            {
                return false;
            }
            foreach (var itemA in enumerableA)
            {
                var itemAFoundInB = false;
                foreach (var itemB in enumerableB)
                {
                    if (itemA is Entity itemAEntity && itemB is Entity itemBEntity)
                    {
                        if (ReflectionEquals(itemAEntity, itemBEntity, comparedEntities, recursionLevels++))
                        {
                            itemAFoundInB = true;
                            break;
                        }
                    }
                    else if (Equals(itemA, itemB))
                    {
                        itemAFoundInB = true;
                    }
                }
                if (!itemAFoundInB)
                {
                    return false;
                }
            }
            foreach (var itemA in enumerableA)
            {
                var itemAFoundInB = false;
                foreach (var itemB in enumerableB)
                {
                    if (itemA is Entity itemAEntity && itemB is Entity itemBEntity)
                    {
                        if (ReflectionEquals(itemAEntity, itemBEntity, comparedEntities, recursionLevels++))
                        {
                            itemAFoundInB = true;
                            break;
                        }
                    }
                    else if (Equals(itemA, itemB))
                    {
                        itemAFoundInB = true;
                    }
                }
                if (!itemAFoundInB)
                {
                    return false;
                }
            }

            return true;
        }


        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "The times where multiple casting occurs below are a big help to code readability")]
        private static bool ReflectionEquals(Entity entityA, Entity entityB, ICollection<Tuple<Entity, Entity>> comparedEntities, int recursionLevels = 0)
        {
#if DEBUG
            if (recursionLevels > 50)
            {
                Debugger.Break();
            }
#endif
            if (ReferenceEquals(entityA, entityB) || (entityA == null && entityB == null))
            {
                return true;
            }

            if (entityA == null || entityB == null || entityA.GetType() != entityB.GetType())
            {
                return false;
            }

            foreach (var property in entityA.GetType().GetRuntimeProperties().Where(property => property.CanRead && !property.GetCustomAttributes<IgnoreInIdentityAttribute>().Any()).OrderBy(property => property.Name))
            {
                var propertyValueA = property.GetValue(entityA);
                var propertyValueB = property.GetValue(entityB);
                if (propertyValueA == null && propertyValueB != null || propertyValueA != null && propertyValueB == null)
                {
                    return false;
                }

                var propertyEntityB = propertyValueB as Entity;

                if (propertyValueA is Entity propertyEntityA)
                {
                    var compare1 = new Tuple<Entity, Entity>(propertyEntityA, propertyEntityB);
                    var compare2 = new Tuple<Entity, Entity>(propertyEntityB, propertyEntityA);
                    if (!comparedEntities.Contains(compare1) && !comparedEntities.Contains(compare2))
                    {
                        comparedEntities.Add(compare1);
                        if (!ReflectionEquals(propertyEntityA, propertyEntityB, comparedEntities, recursionLevels++))
                        {
                            return false;
                        }
                    }
                }
                else if (propertyValueA is IEnumerable enumerable && !(enumerable is string))
                {
                    var enumerableA = enumerable.Cast<object>();
                    var enumerableB = (propertyValueB as IEnumerable).Cast<object>();
                    if (!ReflectionEquals(enumerableA, enumerableB, comparedEntities, recursionLevels++))
                    {
                        return false;
                    }
                }
                else if (!Equals(propertyValueA, propertyValueB))
                {
                    return false;
                }
            }

            return true;
        }

        public object Clone(CloneDepth depth) => depth == CloneDepth.Deep ? DeepCloner.Clone(this) : MemberwiseClone();

        public override bool Equals(object obj) => obj is Entity otherEntity && ReflectionEquals(this, otherEntity, new List<Tuple<Entity, Entity>>(), 0);

        public override int GetHashCode() => GetType().GetHashCode();

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(GetType().FullName);
            ReflectionToString(stringBuilder, 1, new List<Entity>());
            return stringBuilder.ToString();
        }

        [NotifyPropertyChangedInvocator]
        [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global", Justification = "False positive -- this should not be possible on a public API")]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            HasChanged = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "nestedCount+1", Justification = "A stack overflow would occur long before we would have 2147483648 nesting levels")]
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "nestedCount-1", Justification = "A stack overflow would occur long before we would have 2147483648 nesting levels")]
        private void ReflectionToString(StringBuilder builder, int nestedCount, ICollection<Entity> outputEntities)
        {
            var indent = new string(' ', (nestedCount - 1) * 4) + "-   ";
            foreach (var property in GetType().GetRuntimeProperties().Where(property => property.CanRead).OrderBy(property => property.Name))
            {
                var propertyValue = property.GetValue(this);
                if (propertyValue == null)
                {
                    builder.AppendLine().AppendFormat(CultureInfo.InvariantCulture, "{0}{1}: null", indent, property.Name);
                }
                else if (propertyValue is Entity entityValue && !outputEntities.Contains(entityValue))
                {
                    outputEntities.Add(entityValue);
                    builder.AppendLine().AppendFormat(CultureInfo.InvariantCulture, "{0}{1}: ({2})", indent, property.Name, property.PropertyType.FullName);
                    entityValue.ReflectionToString(builder, nestedCount + 1, outputEntities);
                }
                else if (property.PropertyType == typeof(string))
                {
                    builder.AppendLine().AppendFormat(CultureInfo.InvariantCulture, "{0}{1}: \"{2}\"", indent, property.Name, propertyValue);
                }
                else
                {
                    builder.AppendLine().AppendFormat(CultureInfo.InvariantCulture, "{0}{1}: {2}", indent, property.Name, propertyValue);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

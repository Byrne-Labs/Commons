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
            InstanceId = Guid.NewGuid();
        }

        public Guid? EntityId { get; set; }

        public bool HasChanged { get; protected set; }

        public Guid InstanceId { get; }

        public bool NeverPersisted { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Code as written greatly improves readability")]
        private static object Clone(object obj, IDictionary<object, object> clonedObjects, CloneDepth depth)
        {
            object clone;
            if (clonedObjects.ContainsKey(obj))
            {
                clone = clonedObjects[obj];
            }
            else if (obj is Entity entity && !clonedObjects.Values.Contains(entity))
            {
                clone = entity.MemberwiseClone();
                clonedObjects.Add(entity, clone);
                foreach (var property in clone.GetType().GetRuntimeProperties().Where(propertyCheck => propertyCheck.CanRead && propertyCheck.CanWrite))
                {
                    var propertyValue = property.GetValue(entity);
                    if (propertyValue != null)
                    {
                        var clonedPropertyValue = Clone(propertyValue, clonedObjects, depth);
                        property.SetValue(clone, clonedPropertyValue);
                    }
                }
            }
            else if (obj is IList list)
            {
                var defaultConstructor = list.GetType().GetTypeInfo().DeclaredConstructors.SingleOrDefault(constructor => constructor.IsPublic && constructor.GetParameters().Length == 0);
                if (defaultConstructor != null)
                {
                    var clonedList = (IList) defaultConstructor.Invoke(Array.Empty<object>());
                    clonedObjects.Add(list, clonedList);
                    foreach (var clonedItem in list.Cast<object>().Select(item => Clone(item, clonedObjects, depth)))
                    {
                        clonedList.Add(clonedItem);
                    }

                    clone = clonedList;
                }
                else
                {
                    clone = list;
                }
            }
            else if (obj.GetType().IsArray)
            {
                var array = (Array) obj;
                var clonedArray = (Array) array.Clone();
                clonedObjects.Add(array, clonedArray);
                for (var index = 0; index < array.Length; index++)
                {
                    var clonedItem = Clone(array.GetValue(index), clonedObjects, depth);
                    clonedArray.SetValue(clonedItem, index);
                }

                clone = clonedArray;
            }
            else if (obj is ICloneable cloneable)
            {
                clone = cloneable.Clone();
                clonedObjects.Add(cloneable, clone);
            }
            else
            {
                clone = obj;
            }

            return clone;
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
            var equals = true;
            if (!ReferenceEquals(entityA, entityB))
            {
                if (entityA is null || entityB is null || entityA.GetType() != entityB.GetType())
                {
                    equals = false;
                }
                else
                {
                    foreach (var property in entityA.GetType().GetRuntimeProperties().Where(property => property.CanRead && !property.GetCustomAttributes<IgnoreInIdentityAttribute>().Any()).OrderBy(property => property.Name))
                    {
                        var propertyValueA = property.GetValue(entityA);
                        var propertyValueB = property.GetValue(entityB);
                        if (propertyValueA == null && propertyValueB != null || propertyValueA != null && propertyValueB == null)
                        {
                            equals = false;
                            break;
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
                                    equals = false;
                                    break;
                                }
                            }
                        }
                        else if (propertyValueA is IEnumerable && !(propertyValueA is string))
                        {
                            var enumerableA = (propertyValueA as IEnumerable).Cast<object>();
                            var enumerableB = (propertyValueB as IEnumerable).Cast<object>();
                            equals = enumerableA.All(enumerableB.Contains) && enumerableB.All(enumerableA.Contains);
                        }
                        else if (!Equals(propertyValueA, propertyValueB))
                        {
                            equals = false;
                            break;
                        }
                    }
                }
            }

            return equals;
        }

        public object Clone(CloneDepth depth) => Clone(this, new Dictionary<object, object>(), depth);

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(GetType().FullName);
            ReflectionToString(stringBuilder, 1, new List<Entity>());
            return stringBuilder.ToString();
        }

        [NotifyPropertyChangedInvocator]
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
                var entityValue = propertyValue as Entity;
                if (propertyValue == null)
                {
                    builder.AppendLine().AppendFormat(CultureInfo.InvariantCulture, "{0}{1}: null", indent, property.Name);
                }
                else if (entityValue != null && !outputEntities.Contains(entityValue))
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

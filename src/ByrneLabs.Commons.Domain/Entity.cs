using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public object Clone(CloneDepth depth) => depth == CloneDepth.Deep ? DeepCloner.Clone(this) : MemberwiseClone();

        public bool Equivalent(object obj) => obj is Entity otherEntity && new EntityEquivalencyComparer().Equals(this, otherEntity);

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

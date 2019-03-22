using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ByrneLabs.Commons
{
    public abstract class HandyObject<T> : ICloneable<T> where T : HandyObject<T>
    {
        public T Clone(CloneDepth depth = CloneDepth.Deep) => depth == CloneDepth.Deep ? (T) DeepCloner.Clone(this) : (T) MemberwiseClone();

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(GetType().FullName);
            ReflectionToString(stringBuilder, 1, new List<T>());
            return stringBuilder.ToString();
        }

        object ICloneable.Clone(CloneDepth depth) => Clone(depth);

        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "nestedCount+1", Justification = "A stack overflow would occur long before we would have 2147483648 nesting levels")]
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "nestedCount-1", Justification = "A stack overflow would occur long before we would have 2147483648 nesting levels")]
        private void ReflectionToString(StringBuilder builder, int nestedCount, ICollection<T> outputEntities)
        {
            var indent = new string(' ', (nestedCount - 1) * 4) + "-   ";
            foreach (var property in GetType().GetRuntimeProperties().Where(property => property.CanRead).OrderBy(property => property.Name))
            {
                var propertyValue = property.GetValue(this);
                if (propertyValue == null)
                {
                    builder.AppendLine().AppendFormat(CultureInfo.InvariantCulture, "{0}{1}: null", indent, property.Name);
                }
                else if (propertyValue is T entityValue && !outputEntities.Contains(entityValue))
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
    }
}

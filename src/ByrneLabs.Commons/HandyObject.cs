using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ByrneLabs.Commons
{
    public abstract class HandyObject : ICloneable
    {
        public object Clone(CloneDepth depth = CloneDepth.Deep) => depth == CloneDepth.Deep ? DeepCloner.Clone(this) : MemberwiseClone();

        public override bool Equals(object obj) => new HandyObjectReflectionEquivalencyComparer().Equals(this, obj as HandyObject);

        public override int GetHashCode()
        {
            unchecked
            {
                return 379 * GetType().GetHashCode();
            }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(GetType().FullName);
            ReflectionToString(stringBuilder, 1, new List<HandyObject>());
            return stringBuilder.ToString();
        }

        private void ReflectionToString(StringBuilder builder, int nestedCount, ICollection<HandyObject> outputEntities)
        {
            var indent = new string(' ', (nestedCount - 1) * 4) + "-   ";
            foreach (var property in GetType().GetRuntimeProperties().Where(property => property.CanRead).OrderBy(property => property.Name))
            {
                var propertyValue = property.GetValue(this);
                if (propertyValue == null)
                {
                    builder.AppendLine().AppendFormat(CultureInfo.InvariantCulture, "{0}{1}: null", indent, property.Name);
                }
                else if (propertyValue is HandyObject handyObjectValue && !outputEntities.Contains(handyObjectValue))
                {
                    outputEntities.Add(handyObjectValue);
                    builder.AppendLine().AppendFormat(CultureInfo.InvariantCulture, "{0}{1}: ({2})", indent, property.Name, property.PropertyType.FullName);
                    handyObjectValue.ReflectionToString(builder, nestedCount + 1, outputEntities);
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

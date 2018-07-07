using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.TestUtilities
{
    [PublicAPI]
    public static class DataGenerator
    {
        public static IEnumerable<object> Generate(Type type, int count, bool deep = true)
        {
            var items = new List<object>(count);
            while (items.Count < count)
            {
                items.Add(Generate(type, deep, false));
            }

            return items;
        }

        public static IEnumerable<T> Generate<T>(int count, bool deep = true) => Generate(typeof(T), count, deep).Cast<T>();

        public static T Generate<T>(bool deep = true) => Generate<T>(1, deep).Single();

        public static object Generate(Type type, bool deep = true) => Generate(type, deep, false);

        private static bool CanGenerate(Type targetType) => targetType.IsSimple() ||
                                                            typeof(IList).GetTypeInfo().IsAssignableFrom(targetType) && targetType.GenericTypeArguments.Length == 1 && CanGenerate(targetType.GenericTypeArguments[0]) ||
                                                            typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(targetType) && targetType.GetTypeInfo().IsInterface && targetType.GenericTypeArguments.Length == 1 && CanGenerate(targetType.GenericTypeArguments[0]) ||
                                                            !targetType.Namespace.StartsWith("System", StringComparison.Ordinal) && targetType.GetTypeInfo().GetConstructor(Array.Empty<Type>()) != null;

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "The large amount of branching creates a high complexity score but the simplicity of each branch is very small.  Splitting up the method would decrease code readability.")]
        [SuppressMessage("ReSharper", "CyclomaticComplexity", Justification = "The large amount of branching creates a high complexity score but the simplicity of each branch is very small.  Splitting up the method would decrease code readability.")]
        private static object Generate(Type type, bool deep, bool inDeep)
        {
            var targetType = type.Namespace.Equals("System", StringComparison.Ordinal) && type.Name.Equals("Nullable`1", StringComparison.Ordinal) ? type.GenericTypeArguments[0] : type;
            object returnValue;
            if (!CanGenerate(targetType))
            {
                returnValue = null;
            }
            else if (targetType.GetTypeInfo().IsEnum)
            {
                returnValue = Enum.GetValues(targetType).Cast<object>().RandomItem();
            }
            else if (typeof(string) == targetType)
            {
                returnValue = Guid.NewGuid().ToString();
            }
            else if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                returnValue = BetterRandom.Next(2) == 1;
            }
            else if (targetType == typeof(short) || targetType == typeof(short?))
            {
                returnValue = BetterRandom.NextShort();
            }
            else if (targetType == typeof(int) || targetType == typeof(int?))
            {
                returnValue = BetterRandom.Next();
            }
            else if (targetType == typeof(double) || targetType == typeof(double?))
            {
                returnValue = BetterRandom.NextDouble();
            }
            else if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                returnValue = BetterRandom.NextDateTime();
            }
            else if (targetType == typeof(byte) || targetType == typeof(byte?))
            {
                returnValue = BetterRandom.NextByte();
            }
            else if (targetType == typeof(byte[]))
            {
                returnValue = BetterRandom.NextBytes(BetterRandom.Next(1, 20));
            }
            else if (deep && typeof(IList).GetTypeInfo().IsAssignableFrom(targetType) && targetType.GenericTypeArguments.Length == 1 && CanGenerate(targetType.GenericTypeArguments[0]) && targetType.GetTypeInfo().GetConstructor(Array.Empty<Type>()) != null)
            {
                var list = (IList) Activator.CreateInstance(targetType);
                var itemType = targetType.GenericTypeArguments[0];
                var count = BetterRandom.Next(1, 10);
                while (list.Count < count)
                {
                    list.Add(Generate(itemType, deep, true));
                }

                returnValue = list;
            }
            else if (deep && typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(targetType) && targetType.GetTypeInfo().IsInterface && targetType.GenericTypeArguments.Length == 1 && CanGenerate(targetType.GenericTypeArguments[0]))
            {
                var itemType = targetType.GenericTypeArguments[0];
                var list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
                var count = BetterRandom.Next(1, 10);
                while (list.Count < count)
                {
                    list.Add(Generate(itemType, deep, true));
                }

                returnValue = list;
            }
            else if (!targetType.Namespace.StartsWith("System", StringComparison.Ordinal))
            {
                returnValue = Activator.CreateInstance(targetType);
                if (deep || !inDeep)
                {
                    foreach (var property in targetType.GetTypeInfo().GetProperties().Where(property => property.CanWrite))
                    {
                        var propertyValue = Generate(property.PropertyType, deep, true);
                        property.SetValue(returnValue, propertyValue);
                    }
                }
            }
            else if (!deep)
            {
                returnValue = null;
            }
            else
            {
                throw new InvalidOperationException("This should never happen with type " + targetType.FullName);
            }

            return returnValue;
        }
    }
}

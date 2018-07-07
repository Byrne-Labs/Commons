using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace ByrneLabs.Commons
{
    [PublicAPI]
    public static class ReflectionHelper
    {
        private static readonly Type[] _simpleTypes = { typeof(string), typeof(bool), typeof(bool?), typeof(short), typeof(short?), typeof(int), typeof(int?), typeof(double), typeof(double?), typeof(DateTime), typeof(DateTime?), typeof(byte), typeof(byte?) };

        public static bool CanBeCastAs(this Type type, Type castType) => type.IsSubclassOf(castType) || type.GetInterfaces().Contains(castType) || type == castType;

        public static bool CanBeCastAs<T>(this Type type) => CanBeCastAs(type, typeof(T));

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Included because it is likely to be used in future")]
        public static bool IsSimple(this object value) => value == null || IsSimple(value.GetType());

        public static bool IsSimple(this Type type) => _simpleTypes.Contains(type) || type.GetTypeInfo().IsEnum || type.IsArray && IsSimple(type.GetElementType()) || typeof(IEnumerable<>).GetTypeInfo().IsAssignableFrom(type) && IsSimple(type.GetTypeInfo().GetGenericArguments()[0]);

        public static bool IsSimpleOrEnumerableOfSimple(this object value) => value == null || IsSimpleOrEnumerableOfSimple(value.GetType());

        public static bool IsSimpleOrEnumerableOfSimple(this Type type) => IsSimple(type) || type.IsArray && IsSimple(type.GetElementType()) || typeof(IEnumerable<>).GetTypeInfo().IsAssignableFrom(type) && IsSimple(type.GetTypeInfo().GetGenericArguments()[0]);
    }
}

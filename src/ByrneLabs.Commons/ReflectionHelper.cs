using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace ByrneLabs.Commons
{
    public static class ReflectionHelper
    {
        private static readonly Type[] SimpleTypes = { typeof(string), typeof(bool), typeof(bool?), typeof(short), typeof(short?), typeof(int), typeof(int?), typeof(double), typeof(double?), typeof(DateTime), typeof(DateTime?), typeof(byte), typeof(byte?) };

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Included because it is likely to be used in future")]
        public static bool IsSimple(object value) => value == null || IsSimple(value.GetType());

        public static bool IsSimple(Type type) => SimpleTypes.Contains(type) || type.GetTypeInfo().IsEnum || type.IsArray && IsSimple(type.GetElementType()) || typeof(IEnumerable<>).GetTypeInfo().IsAssignableFrom(type) && IsSimple(type.GetTypeInfo().GetGenericArguments()[0]);

        public static bool IsSimpleOrEnumerableOfSimple(object value) => value == null || IsSimpleOrEnumerableOfSimple(value.GetType());

        public static bool IsSimpleOrEnumerableOfSimple(Type type) => IsSimple(type) || type.IsArray && IsSimple(type.GetElementType()) || typeof(IEnumerable<>).GetTypeInfo().IsAssignableFrom(type) && IsSimple(type.GetTypeInfo().GetGenericArguments()[0]);
    }
}

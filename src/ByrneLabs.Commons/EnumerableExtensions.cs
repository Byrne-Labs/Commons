using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ByrneLabs.Commons
{
    [PublicAPI]
    public static class EnumerableExtensions
    {
        private sealed class InstanceComparer<T> : IEqualityComparer<T>
        {
            public bool Equals(T x, T y) => ReferenceEquals(x, y);

            public int GetHashCode(T obj) => 0;
        }

        public static IEnumerable<T> DistinctInstances<T>(this IEnumerable<T> enumeration) => enumeration.Distinct(new InstanceComparer<T>());
    }
}

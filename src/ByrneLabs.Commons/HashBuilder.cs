using System.Collections;
using System.Linq;

namespace ByrneLabs.Commons
{
    public static class HashBuilder
    {
        public static int Hash(params object[] values)
        {
            unchecked
            {
                int hash;
                var nonNullValues = values.Where(value => value != null).ToArray();
                if (nonNullValues.Length == 0)
                {
                    hash = 0;
                }
                else
                {
                    hash = (int) 2166136261;

                    foreach (var value in values.Where(value => value != null))
                    {
                        if (value is IEnumerable enumerable)
                        {
                            hash = enumerable.Cast<object>().Aggregate(hash, (current, nestedValue) => current * 16777619 ^ nestedValue.GetHashCode());
                        }
                        else
                        {
                            hash = hash * 16777619 ^ value.GetHashCode();
                        }
                    }
                }

                return hash;
            }
        }
    }
}

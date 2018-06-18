using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Xunit;

namespace ByrneLabs.Commons.TestUtilities.XUnit
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "There are currently some unused methods that are logical additions and will likely be used in the future")]
    public static class BetterAssert
    {
        public static void ContainsSame<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            ContainsSame(expected, actual, null, null);
        }

        public static void ContainsSame<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message, params object[] args)
        {
            Assert.Collection(actual, actualItem => Assert.True(expected.Contains(actualItem), string.Format(CultureInfo.InvariantCulture, message, args)));
            Assert.Collection(expected, expectedItem => Assert.True(actual.Contains(expectedItem), string.Format(CultureInfo.InvariantCulture, message, args)));
        }

        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "Suggested change would mean potentially requring caller to create an array to clarify if parameter was a message or format arguement")]
        public static void IsNotEmpty(object value)
        {
            IsNotEmpty(value, null);
        }

        public static void IsNotEmpty(object value, string message, params object[] args)
        {
            Assert.NotNull(value);
            if (value is string stringValue)
            {
                Assert.False(string.IsNullOrEmpty(stringValue), string.Format(CultureInfo.InvariantCulture, message, args) ?? "String is empty");
            }
            else if (value is IEnumerable enumerableValue)
            {
                Assert.True(enumerableValue.Cast<object>().Any(), string.Format(CultureInfo.InvariantCulture, message, args) ?? "Enumerable is empty");
            }
        }

        public static T IsType<T>(object value)
        {
            Assert.NotNull(value);
            Assert.Equal(typeof(T), value.GetType());
            return (T) value;
        }
    }
}

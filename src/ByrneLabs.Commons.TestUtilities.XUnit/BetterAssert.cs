using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Xunit;
using Xunit.Sdk;

namespace ByrneLabs.Commons.TestUtilities.XUnit
{
    [PublicAPI]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "There are currently some unused methods that are logical additions and will likely be used in the future")]
    public class BetterAssert : Assert
    {
        public static void ContainsSame<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            ContainsSame(expected, actual, "Expected '{0}' but was actually '{1}'", string.Join(",", expected.Select(expectedItem => expectedItem.ToString())), string.Join(",", actual.Select(actualItem => actualItem.ToString())));
        }

        public static void ContainsSame<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message, params object[] args)
        {
            True(expected == null && actual == null || expected != null && actual != null && expected.Count() == actual.Count());
            foreach (var actualItem in actual)
            {
                True(expected.Any(expectedItem => object.Equals(actualItem, expectedItem)), string.Format(CultureInfo.InvariantCulture, message, args));
            }

            foreach (var expectedItem in expected)
            {
                True(actual.Any(actualItem => object.Equals(expectedItem, actualItem)), string.Format(CultureInfo.InvariantCulture, message, args));
            }
        }

        public static void Count(int expected, IEnumerable value)
        {
            var count = value.Cast<object>().Count();
            if (expected != count)
            {
                throw new AssertCollectionCountException(expected, count);
            }
        }

        [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global", Justification = "Suggested change would mean potentially requring caller to create an array to clarify if parameter was a message or format arguement")]
        public static void IsNotEmpty(object value)
        {
            IsNotEmpty(value, null);
        }

        public static void IsNotEmpty(object value, string message, params object[] args)
        {
            NotNull(value);
            if (value is string stringValue)
            {
                False(string.IsNullOrEmpty(stringValue), string.Format(CultureInfo.InvariantCulture, message, args) ?? "String is empty");
            }
            else if (value is IEnumerable enumerableValue)
            {
                True(enumerableValue.Cast<object>().Any(), string.Format(CultureInfo.InvariantCulture, message, args) ?? "Enumerable is empty");
            }
        }
    }
}

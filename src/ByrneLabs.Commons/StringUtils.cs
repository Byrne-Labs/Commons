using System;
using System.Collections.Generic;
using System.Text;
using EnsureThat;
using JetBrains.Annotations;

namespace ByrneLabs.Commons
{
    [PublicAPI]
    public static class StringUtils
    {
        public static int ContainsCount(this string value, string substring, StringComparison stringComparison = StringComparison.Ordinal)
        {
            var currentIndex = 0;
            var count = 0;
            while (currentIndex <= value.Length - substring.Length && currentIndex >= 0)
            {
                currentIndex = value.IndexOf(substring, currentIndex, stringComparison);
                if (currentIndex >= 0)
                {
                    count++;
                    currentIndex = currentIndex + substring.Length;
                }
            }

            return count;
        }

        public static (int, int) GetLineAndColumnNumber(this string value, int index)
        {
            Ensure.That(value).IsNotNull();
            Ensure.That(index).IsGte(0);
            Ensure.That(index).IsLt(value.Length);

            var line = value.Substring(0, index).ContainsCount("\n") + 1;
            var column = line == 1 ? index + 1 : index - value.NthIndexOf("\n", line - 1);

            return (line, column);
        }

        public static string Join(this IEnumerable<string> value, string seperator) => string.Join(seperator, value);

        public static int NthIndexOf(this string value, string substring, int count, StringComparison stringComparison = StringComparison.Ordinal)
        {
            Ensure.That(value).IsNotNull();
            Ensure.That(substring).IsNotNullOrEmpty();
            Ensure.That(count).IsGt(0);

            var foundCount = 0;
            var index = 0;
            var nextIndex = 0;

            do
            {
                index = value.IndexOf(substring, nextIndex, stringComparison);
                if (index >= 0)
                {
                    foundCount++;
                    nextIndex = index + substring.Length;
                }
            } while (index >= 0 && foundCount < count);

            return index;
        }

        public static string Repeat(this string value, int count) => new StringBuilder(value.Length * count).Insert(0, value, count).ToString();

        public static string SubstringAfterLast(this string value, string substring, StringComparison stringComparison = StringComparison.Ordinal)
        {
            Ensure.That(value).IsNotNullOrEmpty();
            Ensure.That(substring).IsNotNullOrEmpty();

            var lastIndex = value.LastIndexOf(substring, stringComparison);
            string result;
            if (lastIndex < 0)
            {
                result = string.Empty;
            }
            else
            {
                result = value.Substring(lastIndex + substring.Length);
            }

            return result;
        }

        public static string SubstringBeforeLast(this string value, string substring, StringComparison stringComparison = StringComparison.Ordinal)
        {
            Ensure.That(value).IsNotNullOrEmpty();
            Ensure.That(substring).IsNotNullOrEmpty();

            var lastIndex = value.LastIndexOf(substring, stringComparison);
            string result;
            if (lastIndex < 1)
            {
                result = string.Empty;
            }
            else
            {
                result = value.Substring(0, lastIndex);
            }

            return result;
        }
    }
}

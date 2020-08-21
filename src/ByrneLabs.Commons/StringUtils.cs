using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
                    currentIndex += substring.Length;
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

        public static bool IsAllLower(this string value) => !string.IsNullOrEmpty(value) && Regex.IsMatch(value, @"[a-z]+");

        public static bool IsAllUpper(this string value) => !string.IsNullOrEmpty(value) && Regex.IsMatch(value, @"[A-Z]+");

        public static string Join(this IEnumerable<string> value, string separator) => string.Join(separator, value);

        public static int NthIndexOf(this string value, string substring, int count, StringComparison stringComparison = StringComparison.Ordinal)
        {
            Ensure.That(value).IsNotNull();
            Ensure.That(substring).IsNotNullOrEmpty();
            Ensure.That(count).IsGt(0);

            var foundCount = 0;
            int index;
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

        public static string SubstringAfterFirst(this string value, string substring)
        {
            var firstIndex = value.IndexOf(substring, StringComparison.Ordinal);
            string result;
            if (firstIndex < 0)
            {
                result = string.Empty;
            }
            else
            {
                result = value.Substring(firstIndex + substring.Length);
            }

            return result;
        }

        public static string SubstringAfterLast(this string value, string substring)
        {
            var lastIndex = value.LastIndexOf(substring, StringComparison.Ordinal);
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
                result = value;
            }
            else
            {
                result = value.Substring(0, lastIndex);
            }

            return result;
        }

        public static string Trim(this string value, string trim) => value.TrimStart(trim).TrimEnd(trim);

        public static string TrimEnd(this string value, string trim)
        {
            Ensure.That(value).IsNotNull();
            Ensure.That(trim).IsNotNull();

            string result;
            if (trim.Length > value.Length)
            {
                result = value;
            }
            else if (value.Substring(value.Length - trim.Length) == trim)
            {
                result = value.Substring(0, value.Length - trim.Length);
            }
            else
            {
                result = value;
            }

            return result;
        }

        public static string TrimStart(this string value, string trim)
        {
            Ensure.That(value).IsNotNull();
            Ensure.That(trim).IsNotNull();

            string result;
            if (trim.Length > value.Length)
            {
                result = value;
            }
            else if (value.Substring(0, trim.Length) == trim)
            {
                result = value.Substring(trim.Length);
            }
            else
            {
                result = value;
            }

            return result;
        }
    }
}

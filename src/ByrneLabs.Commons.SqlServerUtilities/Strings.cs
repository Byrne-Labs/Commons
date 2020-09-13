using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SqlServer.Server;

namespace ByrneLabs.Commons.SqlServerUtilities
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class UserDefinedFunctions
    {
        [SqlFunction]
        public static string SubstringAfterFirst(string value, string substring)
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

        [SqlFunction]
        public static string SubstringAfterLast(string value, string substring)
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

        [SqlFunction]
        public static string SubstringBeforeFirst(string value, string substring)
        {
            var firstIndex = value.IndexOf(substring, StringComparison.Ordinal);
            string result;
            if (firstIndex < 1)
            {
                result = value;
            }
            else
            {
                result = value.Substring(0, firstIndex);
            }

            return result;
        }

        [SqlFunction]
        public static string SubstringBeforeLast(string value, string substring)
        {
            var lastIndex = value.LastIndexOf(substring, StringComparison.Ordinal);
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

        [SqlFunction]
        public static string TrimEnd(string value, string trim)
        {
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

        [SqlFunction]
        public static string TrimStart(string value, string trim)
        {
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

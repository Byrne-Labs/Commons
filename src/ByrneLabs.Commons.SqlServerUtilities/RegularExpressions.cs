using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

namespace ByrneLabs.Commons.SqlServerUtilities
{
    public partial class UserDefinedFunctions
    {
        [SqlFunction(IsDeterministic = true, IsPrecise = true, TableDefinition = "Match NVARCHAR(4000)", FillRowMethodName = "MatchesFillRow")]
        public static IEnumerable AllMatches(string input, string pattern)
        {
            IEnumerable returnValue;
            if (input == null || pattern == null)
            {
                returnValue = new List<string>();
            }
            else
            {
                var captures = new List<string>();
                var matches = Regex.Matches(input, pattern, RegexOptions.IgnoreCase).Cast<Match>();
                foreach (var match in matches)
                {
                    for (var groupIndex = 1; groupIndex < match.Groups.Count; groupIndex++)
                    {
                        var group = match.Groups[groupIndex];
                        captures.AddRange(group.Captures.Cast<Capture>().Select(capture => capture.Value));
                    }
                }

                returnValue = captures;
            }

            return returnValue;
        }

        [SqlFunction(IsDeterministic = true, IsPrecise = true)]
        public static bool IsMatch(string input, string pattern) => input != null && pattern != null && Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);

        [SqlFunction(IsDeterministic = true, IsPrecise = true)]
        public static string Match(string input, string pattern, int groupNumber) => input == null || pattern == null ? null : Regex.Match(input, pattern, RegexOptions.IgnoreCase).Groups[groupNumber].Value;

        [SqlFunction(IsDeterministic = true, IsPrecise = true, TableDefinition = "Match NVARCHAR(4000)", FillRowMethodName = "MatchesFillRow")]
        public static IEnumerable Matches(string input, string pattern, int groupNumber)
        {
            return input == null || pattern == null ? new List<string>() : Regex.Matches(input, pattern, RegexOptions.IgnoreCase).Cast<Match>().SelectMany(match => match.Groups[groupNumber].Captures.Cast<Capture>().Select(capture => capture.Value));
        }

        public static void MatchesFillRow(object match, out SqlString returnMatch)
        {
            returnMatch = ((string) match).Length > 4000 ? null : new SqlString((string) match);
        }

        [SqlFunction(IsDeterministic = true, IsPrecise = true)]
        public static string Replace(string input, string pattern, string replacement) => input == null || pattern == null ? null : Regex.Replace(input, pattern, replacement, RegexOptions.IgnoreCase);
    }
}

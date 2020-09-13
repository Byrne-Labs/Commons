using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;

namespace ByrneLabs.Commons.SqlServerUtilities
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class UserDefinedFunctions
    {
        [SqlFunction(IsDeterministic = true, IsPrecise = true, TableDefinition = "MatchId INT, GroupIndex INT, Match NVARCHAR(4000)", FillRowMethodName = "AllMatchesFillRow")]
        public static IEnumerable AllMatches(string input, string pattern)
        {
            var allMatches = new List<Tuple<int, int, string>>();
            if (input != null && pattern != null)
            {
                var matches = Regex.Matches(input, pattern, RegexOptions.IgnoreCase).Cast<Match>();
                var matchId = 1;
                foreach (var match in matches.Where(m => m.Groups.Count > 0))
                {
                    for (var groupIndex = 1; groupIndex < match.Groups.Count; groupIndex++)
                    {
                        var group = match.Groups[groupIndex];
                        allMatches.Add(new Tuple<int, int, string>(matchId, groupIndex, group.Value));
                    }

                    matchId++;
                }
            }

            return allMatches;
        }

        public static void AllMatchesFillRow(object match, out SqlInt32 returnMatchId, out SqlInt32 returnGroupIndex, out SqlString returnMatch)
        {
            var matchTuple = (Tuple<int, int, string>) match;
            returnMatchId = matchTuple.Item1;
            returnGroupIndex = matchTuple.Item2;
            returnMatch = matchTuple.Item3.Length > 4000 ? null : new SqlString(matchTuple.Item3);
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

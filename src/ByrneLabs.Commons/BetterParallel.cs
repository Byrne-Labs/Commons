using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace ByrneLabs.Commons
{
    /*
     * Idea from https://devblogs.microsoft.com/pfxteam/implementing-parallel-while-with-parallel-foreach/
     */
    [PublicAPI]
    public static class BetterParallel
    {
        public static void While(Func<bool> condition, Action body) => While(new ParallelOptions(), condition, body);

        public static void While(ParallelOptions parallelOptions, Func<bool> condition, Action body)
        {
            Parallel.ForEach(IterateUntilFalse(condition), parallelOptions, ignored => body());
        }

        private static IEnumerable<bool> IterateUntilFalse(Func<bool> condition)
        {
            while (condition())
            {
                yield return true;
            }
        }
    }
}

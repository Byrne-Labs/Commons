using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ByrneLabs.Commons
{
    public static class TaskExtensions
    {
        public static void WaitAll(this IEnumerable<Task> tasks)
        {
            int taskCount;
            do
            {
                taskCount = tasks.Count();
                Task.WaitAll(tasks.ToArray());
            } while (taskCount < tasks.Count());
        }
    }
}

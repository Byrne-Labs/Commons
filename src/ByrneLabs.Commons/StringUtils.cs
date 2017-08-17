using System.Text;

namespace ByrneLabs.Commons
{
    public static class StringUtils
    {
        public static string Repeat(this string value, int count) => new StringBuilder(value.Length * count).Insert(0, value, count).ToString();
    }
}

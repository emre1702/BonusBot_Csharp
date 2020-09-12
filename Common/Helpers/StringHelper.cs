using System;
using System.Collections.Generic;

namespace Common.Helpers
{
    public static class StringHelper
    {
        public static IEnumerable<string> SplitByLength(this string str, int length)
        {
            for (int i = 0; i <= str.Length; i += length)
                yield return str.Substring(i, length);
        }
    }
}

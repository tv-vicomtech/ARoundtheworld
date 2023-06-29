using System.Collections.Generic;
using System;
using System.Linq;

namespace OrkestraLib
{
    namespace Utilities
    {
        public static class StringExtensions
        {
            private static readonly Random random = new Random();

            public static List<Action<string>> Splice(this List<Action<string>> source, int index, int count)
            {
                var items = source.GetRange(index, count);
                source.RemoveRange(index, count);
                return items;
            }

            public static List<string> Splice(this List<string> source, int index, int count)
            {
                var items = source.GetRange(index, count);
                source.RemoveRange(index, count);
                return items;
            }

            public static string RandomString(int length)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            }
        }
    }
}
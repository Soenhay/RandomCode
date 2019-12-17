using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public static class ListHelper
    {
    }

    public static class ListExtensions
    {
        /// <summary>
        /// Returns a comma delimited string with no start or end string.
        /// <para>Example: 1,2,3,4 </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToDelimitedString<T>(this List<T> list)
        {
            return list.ToDelimitedString("", ",", "");
        }

        /// <summary>
        /// Example:
        /// <para>Call: (new[] { "1", "2", "3", "4" }.ToList()).ToDelimetedString("(", ",", ")");</para>
        /// <para>Returns: "(1,2,3,4)"</para>
        /// <para>NOTE: .ToList() requires System.Linq.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="startString"></param>
        /// <param name="delimeter"></param>
        /// <param name="endString"></param>
        /// <returns></returns>
        public static string ToDelimitedString<T>(this List<T> list, string startString, string delimeter, string endString)
        {
            string result = startString;

            StringBuilder builder = new StringBuilder();
            foreach (var obj in list)
            {
                builder.Append(obj.ToString());

                if (list.IndexOf(obj) < list.Count - 1)
                    builder.Append(delimeter);
            }
            result += builder.ToString();

            result += endString;
            return result;
        }

        public static List<T> AddUnique<T>(this List<T> list, T item) where T : IComparable
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }

            return list;
        }

        public static List<T> AddRangeUnique<T>(this List<T> list, IEnumerable<T> range) where T : IComparable
        {
            foreach (T item in range)
            {
                list.AddUnique(item);
            }

            return list;
        }
    }
}

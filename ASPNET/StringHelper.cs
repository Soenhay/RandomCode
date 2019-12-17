using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common
{
    public static class StringHelper
    {
    }

    public static class StringExtensions
    {
        /// <summary>
        /// Try to parse the string into a case insensitive dictionary. Both the key and value will retain their original case. When comparing, however, case will be ignored.
        /// <para>The passed in string must be of the form "|KEY=VALUE|KEY=VALUE|"</para>
        /// </summary>
        /// <remarks>
        /// <para>http://stackoverflow.com/questions/1852200/how-to-split-string-into-a-dictionary</para>
        /// <para>http://www.dotnetperls.com/case-insensitive-dictionary</para>
        /// </remarks>
        /// <param name="str">The string to parse.</param>
        /// <returns></returns>
        public static Dictionary<string, string> toDictionary(this String str)
        {
            char outerDelimiterLeft = '|';
            char outerDelimiterRight = '|';
            char innerDelimiter = '=';

            //This LINQ expression does not currently remove duplicate keys so you must make sure the passed in string does not have duplicate keys or this will fail.
            Dictionary<string, string> dict = (from item in str.Split(new[] { outerDelimiterLeft, outerDelimiterRight }, StringSplitOptions.RemoveEmptyEntries)
                                               select new { kvp = item.Split(innerDelimiter) }).ToDictionary(key => key.kvp[0], val => val.kvp[1], StringComparer.OrdinalIgnoreCase);

            #region This way also works and does not use linq
            //Dictionary<string, string> dict = new Dictionary<string, string>();
            //char[] delimsOuter = {outerDelimiterLeft, outerDelimiterRight };
            //char[] delimsInner = {innerDelimiter };

            //string[] partsOuter = str.Split(delimsOuter, StringSplitOptions.RemoveEmptyEntries);

            //foreach (string part in partsOuter)
            //{
            //    string[] partsInner = part.Split(delimsInner, StringSplitOptions.RemoveEmptyEntries);

            //    string key = partsInner.Length > 0 ? partsInner[0] : "";
            //    string value = partsInner.Length > 1 ? partsInner[1] : "";
            //    if (!String.IsNullOrEmpty(key) && !dict.ContainsKey(key))
            //    {
            //        dict.Add(key, value);
            //    }
            //}
            #endregion

            return dict;
        }

        /// <summary>
        /// Truncate the string to be no longer than maxLength.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        /// <remarks>http://stackoverflow.com/questions/3566830/what-method-in-the-string-class-returns-only-the-first-n-characters</remarks>
        public static string TruncateLongString(this string str, int maxLength)
        {
            return str.Substring(0, Math.Min(str.Length, maxLength));
        }

        /// <summary>
        /// http://stackoverflow.com/questions/1722334/c-extract-only-right-most-n-letters-from-a-string
        /// </summary>
        /// <param name="sValue"></param>
        /// <param name="iMaxLength"></param>
        /// <returns></returns>
        public static string Right(this string sValue, int iMaxLength)
        {
            //Check if the value is valid
            if (string.IsNullOrEmpty(sValue))
            {
                //Set valid empty string as string could be null
                sValue = string.Empty;
            }
            else if (sValue.Length > iMaxLength)
            {
                //Make the string no longer than the max length
                sValue = sValue.Substring(sValue.Length - iMaxLength, iMaxLength);
            }

            //Return the string
            return sValue;
        }

        /// <summary>
        /// Converts the string to be only numbers.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToDigitsOnly(this string str)
        {
            Regex r = new Regex(@"[^\d]", RegexOptions.IgnoreCase);
            string result = r.Replace(str, "");
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Find"></param>
        /// <param name="Replace"></param>
        /// <returns></returns>
        /// <remarks>http://stackoverflow.com/questions/14825949/replace-the-last-occurence-of-a-word-in-a-string-c-sharp</remarks>
        public static string ReplaceLastOccurrence(this string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return string.Empty;

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }
    }
}

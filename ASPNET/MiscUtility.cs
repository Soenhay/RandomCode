using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace SoenhaysLibrary
{
    public static class MiscUtility
    {
        public static bool isDebug()
        {
            // return HttpContext.Current.IsDebuggingEnabled;
            #if DEBUG
            return true;
            #else
            return false;
            #endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min">Inclusive</param>
        /// <param name="max">Inclusive</param>
        /// <returns></returns>
        public static int Random(Int32 min, Int32 max)
        {
            Random rand = new Random((Int32)DateTime.Now.Ticks);
            return rand.Next(min, max + 1);
        }

        public static DateTime getBuildDate()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime result = (new DateTime(2000, 1, 1)).AddDays(version.Build).AddSeconds(version.Revision * 2);
            if (DateTime.Now.IsDaylightSavingTime())
            {
                result = result.AddHours(1);
            }
            return result;
        }

        /// <summary>
        /// Return string with Version number - then build date
        /// </summary>
        /// <returns></returns>
        public static string GetVersionString()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return version + " - " + MiscUtility.getBuildDate().ToString("MM/dd/yyyy - HH:mm");
        }
    }
}

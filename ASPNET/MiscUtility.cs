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
        
                /// <summary>
        /// Returns true if the object is a number, false otherwise. Note that we also have an extension method for the string class that does the same thing.
        /// </summary>
        /// <remarks>http://stackoverflow.com/questions/437882/what-is-the-c-sharp-equivalent-of-nan-or-isnumeric</remarks>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Boolean isNumeric(object value)
        {
            String strValue = SafeConvert.SafeString(value, "NaN");
            if (strValue == "NaN" || strValue == "Infinity") return false;

            long myNum = 0;
            return long.TryParse(strValue, out myNum);
        }

        /// <summary>
        /// Replaces text in a file. Uses Regex.Replace.
        /// <para>Opens and closes a StreamReader and a StreamWriter.</para>
        /// <para>Returns True on success, False on failure. A log is generated on failure.</para>
        /// <para>http://www.csharp411.com/searchreplace-in-files/ </para>
        /// </summary>
        /// <param name="filePath">Path of the text file.</param>
        /// <param name="searchText">Text to search for.</param>
        /// <param name="replaceText">Text to replace the search text.</param>
        /// <returns></returns>
        public static Boolean ReplaceInFile(String filePath, String searchText, String replaceText)
        {
            Boolean success = false;

            try
            {
                StreamReader reader = new StreamReader(filePath);
                string content = reader.ReadToEnd();
                reader.Close();

                content = Regex.Replace(content, searchText, replaceText);

                StreamWriter writer = new StreamWriter(filePath);
                writer.Write(content);
                writer.Close();

                success = true;
            }
            catch (Exception ex)
            {
                LogHelper.ExceptionLog(MethodInfo.GetCurrentMethod(), ex.Message);
            }

            return success;
        }

        /// <summary>
        /// This overload of DeleteDirFilesWithPattern allows you to choose the SearchPattern and 
        /// whether or not to include subdirectories.
        /// <para>Set SearchPattern = "" for default of "*.*" </para>
        /// <para>Set SearchPattern = "*" for everthing.</para>
        /// </summary>
        /// <param name="DirPath"></param>
        /// <param name="SearchPattern"></param>
        /// <param name="IncludeSubDirs"></param>
        /// <returns></returns>
        public static Boolean DeleteDirFilesWithPattern(String DirPath, String SearchPattern, Boolean IncludeSubDirs)
        {
            Boolean success = true;
            SearchOption searchOpt = IncludeSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            SearchPattern = SearchPattern == "" ? "*.*" : SearchPattern;

            foreach (String fileReturned in Directory.GetFiles(DirPath, SearchPattern, searchOpt))
            {
                try
                {
                    File.Delete(fileReturned);
                }
                catch (Exception e)
                {
                    LogHelper.ExceptionLog(MethodBase.GetCurrentMethod(), "Delete File Error for :" + fileReturned + ":" + e.Message);
                    success = false;
                }
            }
            return success;
        }

        /// <summary>
        /// If the directory does not exist then create it. If clearDestination then empty files and sub directories.
        /// <para>Returns true on success, false otherwise.</para>
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="clearDestination"></param>
        public static bool CreateDirectory(string destination, bool clearDestination)
        {
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);//Create directory if it does not exist
            }
            else if (clearDestination)
            {
                (new DirectoryInfo(destination)).Empty();//If it already exists and we want to clear it....Empty the directory of all files and sub directories.
            }

            return Directory.Exists(destination);
        }

        /// <summary>
        /// Returns true if path is a directory, false otherwise.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool isDirectory(string path)
        {
            //This will fail if the path does not exist. Only use it on supposedly existing paths.
            try
            {
                FileAttributes attr = File.GetAttributes(path);
                return ((attr & FileAttributes.Directory) == FileAttributes.Directory);
            }
            catch (Exception ex)
            {
                LogHelper.ExceptionLog(MethodBase.GetCurrentMethod(), ex.Message);
                return false;
            }
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
        
        /// <summary>
        /// hh:mm:ss
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static String FormatTimeSpan(TimeSpan span)
        {
            return
                   span.Hours.ToString("00") + ":" +
                   span.Minutes.ToString("00") + ":" +
                   span.Seconds.ToString("00") + " (h:m:s)";
        }
    }
    

    public static class MiscExtensions
    {
        /// <summary>
        /// Empty the directory of all files and sub directories.
        /// <para>Example Usage: (new DirectoryInfo(strDirectoryPath)).Empty()</para>
        /// </summary>
        /// <param name="directory"></param>
        /// <remarks>http://stackoverflow.com/questions/1288718/how-to-delete-all-files-and-folders-in-a-directory</remarks>
        public static void Empty(this System.IO.DirectoryInfo directory)
        {
            if (directory.Exists)
            {
                foreach (System.IO.FileInfo file in directory.GetFiles()) { file.Delete(); }
                foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) { subDirectory.Delete(true); }
            }
        }
    }
}

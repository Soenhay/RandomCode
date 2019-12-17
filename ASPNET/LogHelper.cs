using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Common
{
    /// <summary>
    /// Methods for assisting with log messages.
    /// </summary>
    public static class LogHelper
    {
        public static class Colors
        {
            public static Color Notice = Color.FromArgb(255, 255, 192, 77);//Lighter orange.
            public static Color Warning = Color.FromArgb(255, 255, 144, 0);//Darker orange.
            public static Color Error = Color.Red;
        }

        public static string GetLogPath(String strDestination, String fileName)
        {
            fileName = fileName.Length == 0 ? "log.txt" : fileName;
            #region web
            //String appPath = HttpContext.Current.Request.ApplicationPath;
            //String physicalPath = HttpContext.Current.Request.MapPath(appPath);
            #endregion web
            #region winforms
            String physicalPath = Application.StartupPath;
            #endregion winforms
            strDestination = String.IsNullOrEmpty(strDestination) ? Path.Combine(physicalPath, fileName) : strDestination;
            return strDestination;
        }

        /// <summary>
        /// Try to open the log file in notepad++. If that fails then try to open it in notepad. If that fails then display a notification.
        /// </summary>
        public static void OpenLogFile()
        {
            string logFilePath = LogHelper.GetLogPath("", "");//Get default log file path.
            string textEditorPath = @"C:\Program Files (x86)\Notepad++\notepad++.exe";
            if (!File.Exists(textEditorPath))
            {
                textEditorPath = @"C:\Program Files\Notepad++\notepad++.exe";
                if (!File.Exists(textEditorPath))
                {
                    string windir = Environment.GetEnvironmentVariable("WINDIR");
                    textEditorPath = windir + @"\notepad.exe";
                }
            }

            try
            {
                System.Diagnostics.Process prc = new System.Diagnostics.Process();
                prc.StartInfo.FileName = textEditorPath;
                prc.StartInfo.Arguments = logFilePath;
                prc.Start();
            }
            catch
            {
                MessageBox.Show("Could Not Open Log File at " + logFilePath + ". Try manually opening the file.", "oops", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// This version of ExceptionLog uses the passed in MethodBase object to create a string for whatFailed.
        /// </summary>
        /// <param name="methodBase">MethodBase.GetCurrentMethod()</param>
        /// <param name="msg"></param>
        /// <returns>Returns the log that was written to the log file.</returns>
        public static string ExceptionLog(MethodBase methodBase, String msg)
        {
            return AddLog("", "", methodBase.ReflectedType.Name + "." + methodBase.Name + " [ERROR]: " + msg);
        }

        /// <summary>
        /// Adds a log entry by appending to the file. Creates a new log file if the current one is too large.
        /// <para>If a destination path string with length 0 is passed in, then the Application.StartupPath is used.</para>
        /// <para>If a fileName string of length 0 is passed in, then log.txt is used.</para>
        /// <para>If a message string with length 0 is passed in, then a blank line is appended to the log.</para>
        /// </summary>
        /// <param name="strDestination">"" for Application.StartupPath.</param>
        /// <param name="fileName">"" for log.txt.</param>
        /// <param name="msg">"" for blank line.</param>
        /// <returns>Returns the log that was written to the log file.</returns>
        /// <remarks>Some code from http://stackoverflow.com/questions/26376253/the-process-cannot-access-the-file-because-it-is-being-used-by-another-process-u </remarks>
        public static string AddLog(String strDestination, String fileName, String msg)
        {
            string log = "";
            strDestination = GetLogPath(strDestination, fileName);

            //Archive it and create a new log file if necessary... Create a log file if one does not exist.
            //Uses StreamWriter on strDestination so it has to happen before another StreamWriter on strDestion is created or after it is closed.
            CreateNewLogFileIfNeeded(strDestination, 50000);

            int maxRetry = 2;
            for (int retry = 0; retry < maxRetry; retry++)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(strDestination, true))
                    {
                        //Write the message to the file.
                        if (msg.Length > 0)
                        {
                            Int32 PID = Process.GetCurrentProcess().Id;
                            log = String.Format("{0} - [PID={1}] - {2}", DateTime.Now, PID, msg);
                            sw.WriteLine(log);
                        }
                        else
                        {
                            sw.WriteLine("");
                        }
                        sw.Close();
                        break;//No need to retry
                    }
                }
                catch
                {
                    //Failed to write to the log file. Or failed to create the file.                

                    if (retry < maxRetry - 1)
                    {
                        System.Threading.Thread.Sleep(500); // Wait some time before retry ( miliseconds )
                    }
                    else
                    {
                        // handle unsuccessfull write attempts or just ignore.
                    }
                }
            }

            return log;
        }

        /// <summary>
        /// If the current log file is too large, then archive it ( by renaming it ) and create a new one.
        /// <para>Return TRUE if the file was archived. Returns FALSE if the file did not need to be archived or failed to be archived.</para>
        /// </summary>
        /// <param name="fnWithPath"></param>
        /// <param name="MaxLogFileSizeInKB"></param>
        public static Boolean CreateNewLogFileIfNeeded(String fnWithPath, Int32 MaxLogFileSizeInKB)
        {
            Boolean result = false;
            String fnDir = Path.GetDirectoryName(fnWithPath);
            String fnOnly = Path.GetFileName(fnWithPath);
            String fnPrefix = Path.GetFileNameWithoutExtension(fnOnly);
            StreamWriter sw;
            String msg = "";

            if (File.Exists(fnWithPath))
            {
                Decimal fileSize = GetFileSize(fnWithPath, "KB");

                if (fileSize > MaxLogFileSizeInKB)
                {
                    try
                    {
                        //Create log archive folder if it doesn't already exist
                        String dirLogArchive = Path.Combine(fnDir, "LogArchive");
                        if (!Directory.Exists(dirLogArchive))
                        {
                            Directory.CreateDirectory(dirLogArchive);
                        }

                        //Write a message to the old log file prior to archiving it.
                        sw = File.AppendText(fnWithPath);
                        msg = "This Log file has reached its maximum size of " + MaxLogFileSizeInKB + "KB";
                        sw.WriteLine("{0} - {1}", DateTime.Now, msg);
                        sw.Close();

                        //Rename the old log file.
                        String oldFileName = Path.Combine(fnDir, fnOnly);
                        String newFileName = Path.Combine(dirLogArchive, fnPrefix + DateTime.Now.ToString("_yyyyMMdd-HHmm") + ".txt");
                        File.Move(oldFileName, newFileName);

                        //Create a new log file.
                        sw = File.AppendText(fnWithPath);
                        msg = "New log file created. Previous log filename: " + newFileName;
                        sw.WriteLine("{0} - {1}", DateTime.Now, msg);
                        sw.Close();

                        result = true;
                    }
                    catch (Exception ex)
                    {
                        //Something went wrong... leave a break point here.
                        msg = ex.Message;
                    }
                }
            }
            else
            {
                sw = File.AppendText(fnWithPath);
                msg = "Log file created.";
                sw.WriteLine("{0} - {1}", DateTime.Now, msg);
                sw.Close();
            }

            return result;
        }

        /// <summary>
        /// Gets a list of filenames from the directory excluding directory information.
        /// Leave pattern blank for all files.
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static List<String> GetFilenamesOnlyFromDirectory(String dir, String pattern)
        {
            //Get a new file list
            List<String> files = new List<String>();
            if (pattern.Length > 0)
                files.AddRange(Directory.GetFiles(dir, pattern));
            else
                files.AddRange(Directory.GetFiles(dir));

            //Remove the directory information from the filenames
            for (int i = 0; i < files.Count; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }

            return files;
        }

        /// <summary>
        /// Get the size of a file in either GB, MB, or KB.
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="Conversion"></param>
        /// <returns></returns>
        public static Decimal GetFileSize(String fn, String Conversion)
        {
            if (!File.Exists(fn))
            {
                return 0;
            }

            // Create new FileInfo object and get the Length.
            FileInfo f = new FileInfo(fn);
            Int64 Bytes = f.Length;

            // Convert Bytes to the appropiate unit of measure.
            switch (Conversion.ToUpper())
            {
                case "GB":
                    return Decimal.Divide(Bytes, 1073741824);
                case "MB":
                    return Decimal.Divide(Bytes, 1048576);
                case "KB":
                    return Decimal.Divide(Bytes, 1024);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// No checking so make sure parameters are correct
        /// </summary>
        /// <param name="cp">Used to create the middle part of the export path</param>
        /// <param name="basePath">First part of the export path</param>
        /// <param name="path1">Used to create the middle part of the export path</param>
        /// <returns></returns>
        public static string GetExportPath(CommonParams cp, string basePath, string path1)
        {
            return GetExportPath(cp, basePath, path1, "");
        }

        /// <summary>
        /// No checking so make sure parameters are correct
        /// </summary>
        /// <param name="cp">Used to create the middle part of the export path</param>
        /// <param name="basePath">First part of the export path</param>
        /// <param name="path1">Used to create the middle part of the export path</param>
        /// <param name="path2">Used to create the middle part of the export path</param>
        /// <returns></returns>
        public static string GetExportPath(CommonParams cp, string basePath, string path1, string path2)
        {
            string pathToCombine = Path.Combine(cp.BuildingID.ToString(), Path.Combine(cp.Grade.ToString(), Path.Combine(path1.Replace(@"\\", @"\"), path2.Replace(@"\\", @"\"))));
            string path = Path.Combine(basePath, pathToCombine);

            return path;
        }

        #region web
        ///// <summary>
        ///// Simple function to write a trace log. Checks if trace IsEnabled and returns if false.
        ///// <para>Make sure that Trace is enabled in web.config.</para>
        ///// <para>View it via browser at /Trace.axd. Or on the page itself if pageOutput = true.</para>
        ///// <para>Be sure to set enable = false before committing, publishing, and when done using or users can see the output.</para>
        ///// <para>Just in case, localOnly = true.</para>
        ///// </summary>
        ///// <param name="enteringFunction"></param>
        //public static void writeTrace(Boolean enteringFunction)
        //{
        //    if (!HttpContext.Current.Trace.IsEnabled)
        //    {
        //        return;
        //    }

        //    String callingFunctionName = "Undetermined method";
        //    string action = enteringFunction ? "Entering" : "Exiting";
        //    try
        //    {
        //        //Determine the name of the calling function. 
        //        //MethodInfo.GetCurrentMethod().Name gets the method name that 
        //        //contains this code.... so it would need to be used as a parameter.
        //        System.Diagnostics.StackTrace stackTrace =
        //        new System.Diagnostics.StackTrace();
        //        callingFunctionName =
        //        stackTrace.GetFrame(1).GetMethod().Name;
        //    }
        //    catch { }
        //    HttpContext.Current.Trace.Write(action, callingFunctionName);
        //}
        #endregion web

        public class LogEventArgs : EventArgs
        {
            public String MethodInfo;
            public String MessageOriginal;
            public String MessageFormatted;
            public TimeSpan ElapsedTime;
            public Color MessageColor;

            public LogEventArgs(MethodBase methodBase, string msg, bool isException)
                : this(methodBase, msg, TimeSpan.MinValue, Color.Black, isException)
            {
            }

            public LogEventArgs(MethodBase methodBase, string msg, Color color, bool isException)
                : this(methodBase, msg, TimeSpan.MinValue, color, isException)
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="methodBase"></param>
            /// <param name="msg"></param>
            /// <param name="elapsedTime">If it is not TimeSpan.MinValue then it will be concatenated to MessageOriginal and MessageFormatted.</param>
            /// <param name="isException">Defaults to false. Used to determine format of MessageFormatted.</param>
            public LogEventArgs(MethodBase methodBase, string msg, TimeSpan elapsedTime, Color color, bool isException)
            {
                this.Init(methodBase, msg, elapsedTime, color, isException);
            }

            public LogEventArgs(MethodBase methodBase, string whatFailed, Exception ex, Color color)
            {
                string msg = whatFailed + " - " + ex.Message;

                this.Init(methodBase, msg, TimeSpan.MinValue, color, true);
            }

            private void Init(MethodBase methodBase, string msg, TimeSpan elapsedTime, Color color, bool isException)
            {
                if (methodBase != null)
                {
                    this.MethodInfo = "[ " + methodBase.ReflectedType.Name + "." + methodBase.Name + " ]";
                }
                this.MessageOriginal = msg;
                if (isException)
                {
                    this.MessageFormatted = "[ ERROR ]" + this.MethodInfo + " [ Failed with message = " + msg + " ]";
                }
                else
                {
                    this.MessageFormatted = this.MethodInfo + (String.IsNullOrEmpty(msg) ? "" : "[ " + msg + " ]");
                }

                if (elapsedTime != TimeSpan.MinValue)
                {
                    string elapsed = "[ Elapsed Time = " + MiscUtility.FormatTimeSpan(elapsedTime) + " ]";
                    this.MessageOriginal += elapsed;
                    this.MessageFormatted += elapsed;
                }
                this.MessageColor = color;
            }
        }
    }
}

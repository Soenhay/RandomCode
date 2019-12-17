using System;
using System.Drawing;
using System.Reflection;
using System.Collections.Generic;

namespace Common
{
    /// <summary>
    /// This class uses Common.LogHelper and provides several methods for logging based on the passed in DoLog event handler reference.
    /// </summary>
    public class Logger
    {
        #region Members

        /// <summary>
        /// Attach an external event to handle logging.
        /// </summary>
        private event EventHandler<LogHelper.LogEventArgs> DoLog;

        #endregion

        #region Initialization

        public Logger(EventHandler<LogHelper.LogEventArgs> _DoLog)
        {
            this.DoLog = _DoLog;
        }

        #endregion

        #region Event Handler callers

        public void raiseDoLog(object sender, LogHelper.LogEventArgs e)
        {
            EventHandler<LogHelper.LogEventArgs> handler = DoLog;
            if (handler != null)
            {
                handler(sender, e);
                LogHelper.AddLog("", "", e.MessageFormatted);
            }
        }

        public void raiseDoLog(LogHelper.LogEventArgs e)
        {
            EventHandler<LogHelper.LogEventArgs> handler = DoLog;
            if (handler != null)
            {
                handler(this, e);
                LogHelper.AddLog("", "", e.MessageFormatted);
            }
        }

        #endregion

        #region Public Methods

        public void doLog(object sender, LogHelper.LogEventArgs e)
        {
            this.raiseDoLog(sender, e);
        }

        public void doLog(string msg)
        {
            this.raiseDoLog(new LogHelper.LogEventArgs(null, msg, TimeSpan.MinValue, Color.Black, false));
        }

        public void doLog(string msg, Color color)
        {
            this.raiseDoLog(new LogHelper.LogEventArgs(null, msg, TimeSpan.MinValue, color, false));
        }

        /// <summary>
        /// Do an elapsed time log with the passed in elapsed time.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="elapsed"></param>
        public void doLog(string msg, TimeSpan elapsed)
        {
            this.raiseDoLog(new LogHelper.LogEventArgs(null, msg, elapsed, Color.Black, false));
        }

        /// <summary>
        /// Do an elapsed time log with the passed in DateTime and DateTime.Now.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="elapsed"></param>
        public void doLog(string msg, DateTime startTime)
        {
            this.raiseDoLog(new LogHelper.LogEventArgs(null, msg, (DateTime.Now - startTime), Color.Black, false));
        }

        public void doLog(MethodBase methodBase, string msg)
        {
            this.raiseDoLog(new LogHelper.LogEventArgs(methodBase, msg, TimeSpan.MinValue, Color.Black, false));
        }

        public void doLog(MethodBase methodBase, Exception ex)
        {
            this.raiseDoLog(new LogHelper.LogEventArgs(methodBase, ex.Message, TimeSpan.MinValue, LogHelper.Colors.Error, true));
        }

        public void doLog(MethodBase methodBase, string whatFailed, Exception ex)
        {
            this.raiseDoLog(new LogHelper.LogEventArgs(methodBase, whatFailed, ex, LogHelper.Colors.Error));
        }

        public void doLog(MethodBase methodBase, string whatFailed, string msg)
        {
            this.raiseDoLog(new LogHelper.LogEventArgs(methodBase, whatFailed, new Exception(msg, null), LogHelper.Colors.Error));
        }

        public void doLog(MethodBase methodBase, string msg, bool isException)
        {
            this.raiseDoLog(new LogHelper.LogEventArgs(methodBase, msg, TimeSpan.MinValue, Color.Black, isException));
        }

        public void doLog(MethodBase methodBase, string msg, TimeSpan elapsed)
        {
            this.raiseDoLog(new LogHelper.LogEventArgs(methodBase, msg, elapsed, Color.Black, false));
        }

        public void doLog(MethodBase methodBase, string msg, Color color)
        {
            this.raiseDoLog(new LogHelper.LogEventArgs(methodBase, msg, TimeSpan.MinValue, color, false));
        }

        public void doLog(MethodBase methodBase, string msg, Color color, bool isException)
        {
            this.raiseDoLog(new LogHelper.LogEventArgs(methodBase, msg, TimeSpan.MinValue, color, isException));
        }

        public void doLog(MethodBase methodBase, string msg, TimeSpan elapsed, Color color, bool isException)
        {
            this.raiseDoLog(new LogHelper.LogEventArgs(methodBase, msg, elapsed, color, isException));
        }

        public void doLogs(List<string> msgs)
        {
            foreach (string msg in msgs)
            {
                this.doLog(msg, Color.Black);
            }
        }

        #endregion

        #region Private Methods

        #endregion
    }
}

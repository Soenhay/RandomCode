using System;

namespace DataAccessLayer.Interface
{
    public interface IApiDalService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="requestor"></param>
        /// <param name="appName"></param>
        void spLogInsert(string type, string controller, string action, string message, string exception, string requestor, string appName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <param name="thread"></param>
        /// <param name="level"></param>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        /// <param name="requestor"></param>
        /// <param name="requestTime"></param>
        /// <param name="responseMillis"></param>
        /// <param name="statusCode"></param>
        /// <param name="method"></param>
        /// <param name="path"></param>
        /// <param name="queryString"></param>
        /// <param name="requestBody"></param>
        /// <param name="responseBody"></param>
        /// <param name="appName"></param>
        void spLogInsert(string type, string controller, string action,
            string thread, string level, string logger,
            string message, string exception,
                string requestor,
               DateTime? requestTime,
               long responseMillis,
               int statusCode,
               string method,
               string path,
               string queryString,
               string requestBody,
               string responseBody,
               string appName);
    }
}

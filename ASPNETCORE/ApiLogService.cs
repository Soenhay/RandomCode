using Common.Helpers;
using DataAccessLayer.Interface;
using System;

namespace WebApi.Services
{
    public static class ApiLogType
    {
        public const string LoginAttempt = "LoginAttempt";
        public const string AccountChange = "AccountChange";
        public const string Info = "Info";
        public const string Exception = "Exception";
        public const string RequestResponse = "RequestResponse";
    }

    public interface IApiLogService
    {
        void AddLog(string type, string controllerName, string actionName, string message, string exception, string requestor, string appName);
        void AddApiLog(string type, string controller, string action,
            string thread, string level, string logger,
            string message, string exception
              , string requestor
              , DateTime? requestTime
              , long responseMillis
              , int statusCode
              , string method
              , string path
              , string queryString
              , string requestBody
              , string responseBody
              , string appName);
    }
    public class ApiLogService : IApiLogService
    {
        private IApiDalService _apiService;
        //private IMapper _mapper;

        public ApiLogService(IApiDalService apiDalService)//, IMapper mapper)
        {
            _apiService = apiDalService;
            // _mapper = mapper;
        }

        public void AddLog(string type, string controllerName, string actionName, string message, string exception, string requestor, string appName)
        {
            _apiService.spLogInsert(type, controllerName, actionName, message, exception, requestor, appName);
        }

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
        /// <param name="appname"></param>
        /// <remarks>https://salslab.com/a/safely-logging-api-requests-and-responses-in-asp-net-core</remarks>
        public void AddApiLog(string type, string controller, string action,
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
              string appname)
        {
            if (path.ToLower().Contains("/account/authenticate"))
            {
                requestBody = "(Request logging disabled for /api/v1/account/authenticate)";
                if (StatusCodeHelper.IsStatusOK(statusCode))
                {
                    //If the request was in the 200's then disable the response log.
                    responseBody = "(Response logging disabled for /api/v1/account/authenticate)";
                }
                else
                {
                    //Log the failed response message.
                }
            }

            if (requestBody?.ToLower().Contains("password") ?? false)
            {
                requestBody = "(Request logging disabled for body containing the word 'password')";
            }

            if (requestBody?.Length > 100)
            {
                requestBody = $"(Truncated to 100 chars) {requestBody.Substring(0, 100)}";
            }

            if (responseBody?.Length > 100)
            {
                responseBody = $"(Truncated to 100 chars) {responseBody.Substring(0, 100)}";
            }

            if (queryString.Length > 100)
            {
                queryString = $"(Truncated to 100 chars) {queryString.Substring(0, 100)}";
            }

            _apiService.spLogInsert(type, controller, action,
               thread, level, logger,
               message, exception,
               requestor,
               requestTime,
               responseMillis,
               statusCode,
               method,
               path,
               queryString,
               requestBody,
               responseBody,
               appname);
        }
    }
}

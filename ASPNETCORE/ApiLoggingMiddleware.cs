using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Services;

namespace WebApi.Middleware
{
    /// <summary>
    /// https://salslab.com/a/safely-logging-api-requests-and-responses-in-asp-net-core
    /// </summary>
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        IApiLogService _apiLogService;
        private readonly ILogger _logger;

        public ApiLoggingMiddleware(RequestDelegate next, IApiLogService apiLogService, ILogger<ApiLoggingMiddleware> logger)
        {
            _next = next;
            _apiLogService = apiLogService;
            _logger = logger;
        }
        
        public async Task Invoke(HttpContext httpContext)
        {
            _logger.LogInformation("ApiLoggingMiddleware.Invoke ******");
            var stopWatch = Stopwatch.StartNew();
            var requestTime = DateTime.Now;// DateTime.UtcNow;
            string level = "";
            string exceptionMessage = "";

            var request = httpContext.Request;
            var appName = request.Headers["AppName"];

            var clientIp = httpContext.Connection.RemoteIpAddress.ToString();
            log4net.LogicalThreadContext.Properties["requestor"] = clientIp;//This is so subsequent calls to logger will include the clientIp. It is possible that calls before this might have the previous ip so we need to test this.

            var requestBodyContent = await ReadRequestBody(request);
            var response = httpContext.Response;
            var originalBodyStream = response.Body;//Save reference to the original response stream.
            string responseBodyContent = null;

            try
            {
                using (var responseBody = new MemoryStream())
                {
                    response.Body = responseBody;//Change the response stream to this new stream.
                    await _next(httpContext);//Complete the request
                    stopWatch.Stop();

                    responseBodyContent = await ReadResponseBody(response);//Read the response into a variable
                    await responseBody.CopyToAsync(originalBodyStream);//Copy the stream to the original response stream.
                }
            }
            catch (Exception ex)
            {
                stopWatch.Stop();
                level = "ERROR";
                exceptionMessage = ex.ToString();
                response.StatusCode = StatusCodes.Status500InternalServerError;
            }

            //TODO: get controller and action from url.
            _apiLogService.AddApiLog(
                ApiLogType.RequestResponse,
                "", // controller
                "", // action
                Thread.CurrentThread.ManagedThreadId.ToString(), // thread
                level, // level
                "", // logger
                "ApiLoggingMiddleware", // message
                exceptionMessage, // exception
                clientIp, // requestor
                requestTime,
                stopWatch.ElapsedMilliseconds,
                response.StatusCode,
                request.Method,
                request.Path,
                request.QueryString.ToString(),
                requestBodyContent,
                responseBodyContent,
                appName);
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableRewind();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            request.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var bodyAsText = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }
    }
}

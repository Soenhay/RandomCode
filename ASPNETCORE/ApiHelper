using Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public static class ApiHelper
    {
        //https://briancaos.wordpress.com/2017/11/03/using-c-httpclient-from-sync-and-async-code/
        //https://stackoverflow.com/questions/44545955/generic-type-jsonconvert-deserializeobjectlisttstring

        //TODO: implement callback method.

        private static readonly HttpClient _httpClient = new HttpClient();

        static ApiHelper()
        {
            SetHeader("AppName", "MyWebApi");
        }

        /// <summary>
        /// Perform an asynchronous Get request and convert the response the generic type.
        /// <para>Use .Result to make it synchronous.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="authToken"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static async Task<ApiResponse<T>> Get<T>(string url, string authToken)
        {
            ApiResponse<T> result = new ApiResponse<T>();
            result.IsSuccess = false;

            //Auth header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            try
            {
                // The actual Get method
                using (var response = await _httpClient.GetAsync($"{url}"))
                {
                    string content = await response.Content.ReadAsStringAsync();

                    result.ReturnMessage = content;
                    result.StatusCode = (int)response.StatusCode;
                    result.IsSuccess = StatusCodeHelper.IsStatusOK((int)response.StatusCode);

                    if (result.IsSuccess)
                    {
                        result.Data = JsonConvert.DeserializeObject<T>(content); // Deserialize json data
                    }
                    else
                    {
                        //Something went wrong. The request completed but it failed.
                        //TODO: log it?
                    }
                }
            }
            catch (Exception ex)
            {
                //Something went wrong. An exception occurred.
                //TODO: log it.
            }

            return result;
        }

        /// <summary>
        /// Perform an asynchronous Post request and convert the response the generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="authToken"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static async Task<ApiResponse<T>> Post<T>(string url, string authToken, string postData)
        {
            ApiResponse<T> result = new ApiResponse<T>();
            result.IsSuccess = false;

            //Auth header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            try
            {
                HttpContent httpContent = new StringContent(postData, Encoding.UTF8, "application/json");

                // The actual Get method
                using (var response = await _httpClient.PostAsync(url, httpContent))
                {
                    string content = await response.Content.ReadAsStringAsync();

                    result.ReturnMessage = content;
                    result.StatusCode = (int)response.StatusCode;
                    result.IsSuccess = StatusCodeHelper.IsStatusOK((int)response.StatusCode);

                    if (result.IsSuccess)
                    {
                        result.Data = JsonConvert.DeserializeObject<T>(content); // Deserialize json data
                    }
                    else
                    {
                        //Something went wrong. The request completed but it failed.
                        //TODO: log it?
                    }
                }
            }
            catch // (Exception ex)
            {
                //Something went wrong. An exception occurred.
                //TODO: log it.
            }

            return result;
        }

        /// <summary>
        /// Perform an asynchronous Put request. Fire and forget.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="authToken"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static async Task Put<T>(string url, string authToken, string postData)
        {
            ApiResponse<T> result = new ApiResponse<T>();

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // The actual put method including error handling
            using (var content = new StringContent(postData))
            {
                var response = await _httpClient.PutAsync(url, content);
                result.StatusCode = (int)response.StatusCode;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }
                else
                {
                    // Do something with the contents, like write the statuscode and
                    // contents to a log file
                    string resultContent = await response.Content.ReadAsStringAsync();
                    // ... write to log
                }
            }
        }

        /// <summary>
        /// The API will verify that the delete actually occured.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public static async Task<ApiResponse<bool>> Delete(string url, string authToken)
        {
            ApiResponse<bool> result = new ApiResponse<bool>();
            result.IsSuccess = false;

            //Auth header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            try
            {
                // The actual Delete method
                using (var response = await _httpClient.DeleteAsync(url))
                {
                    string content = await response.Content.ReadAsStringAsync();

                    result.ReturnMessage = content;
                    result.StatusCode = (int)response.StatusCode;
                    result.IsSuccess = StatusCodeHelper.IsStatusOK((int)response.StatusCode);

                    if (result.IsSuccess)
                    {
                        //Status code 200 means the delete occurred.
                        result.Data = true;
                    }
                    else
                    {
                        //Something went wrong. The request completed but the delete did not occur.
                        //Reasons include:
                        //Unauthorized.
                        //NotFound.
                    }
                }
            }
            catch // (Exception ex)
            {
                //Something went wrong. An exception occurred.
                //TODO: log it.
            }

            return result;
        }

        public static void SetHeader(string headerName, string headerValue)
        {
            if (_httpClient.DefaultRequestHeaders.Contains(headerName))
            {
                _httpClient.DefaultRequestHeaders.Remove(headerName);
            }
            _httpClient.DefaultRequestHeaders.Add(headerName, headerValue);
        }
    }
}

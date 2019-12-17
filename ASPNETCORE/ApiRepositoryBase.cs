using Common.Helpers;
using Common.Models;
using Repository.Contracts;
using System.Threading.Tasks;

namespace Repository
{
    public abstract class ApiRepositoryBase: IApiRepositoryBase
    {
        string baseUrl;
        string authToken;

        public ApiRepositoryBase(string baseUrl, string authToken)
        {
            this.baseUrl = baseUrl;
            if (!this.baseUrl.EndsWith("/"))
            {
                this.baseUrl = this.baseUrl + "/";
            }
            this.authToken = authToken;
        }

        public string GetUrl(string endPoint)
        {
            if (endPoint.StartsWith("/"))
            {
                endPoint = endPoint.Substring(1);
            }
            return baseUrl + endPoint;
        }

        public Task<ApiResponse<bool>> Delete(string endPoint)
        {
            string url = GetUrl(endPoint);
            return ApiHelper.Delete(GetUrl(endPoint), authToken);
        }

        public Task<ApiResponse<T>> Get<T>(string endPoint)
        {
            string url = GetUrl(endPoint);
            return ApiHelper.Get<T>(url, authToken);
        }

        public Task<ApiResponse<T>> Post<T>(string endPoint, string postData)
        {
            string url = GetUrl(endPoint);
            return ApiHelper.Post<T>(url, authToken, postData);
        }

        public Task Put<T>(string endPoint, string postData)
        {
            string url = GetUrl(endPoint);
            return ApiHelper.Put<T>(url, authToken, postData);
        }
    }
}

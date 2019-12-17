using Common.Models;
using System.Threading.Tasks;

namespace Repository.Contracts
{
    public interface IApiRepositoryBase
    {
        Task<ApiResponse<T>> Get<T>(string endPoint);
        Task<ApiResponse<T>> Post<T>(string endPoint, string postData);
        Task Put<T>(string endPoint, string postData);
        Task<ApiResponse<bool>> Delete(string endPoint);
    }
}

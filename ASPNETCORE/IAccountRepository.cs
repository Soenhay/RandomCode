using Common.Models;
using Repository.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Contracts
{
    //Matches the Api AccountController endpoints.
    public interface IAccountRepository : IApiRepositoryBase
    {
        Task<ApiResponse<User>> Authenticate_JwtToken(string username, string password);
        //public User Authenticate_AccessToken(string username, string password);//Probably don't want to use this one in the website.
        Task<ApiResponse<User>> Create(User user);
        Task<ApiResponse<bool>> Delete(int userId);
        Task<ApiResponse<List<User>>> GetAll(string filter = "");
        Task<ApiResponse<User>> GetById(int userId);
        Task<ApiResponse<User>> GetByUsername(string username);
        Task<ApiResponse<List<UserRole>>> GetUserRoles(int userId, string roleName = "");
        Task<ApiResponse<List<Role>>> GetApplicationRoles(string roleName = "");
        void Update(User user);
    }
}

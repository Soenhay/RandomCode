using Common.Models;
using Newtonsoft.Json;
using Repository.Contracts;
using Repository.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository
{
    public class AccountRepository : ApiRepositoryBase, IAccountRepository
    {
        public AccountRepository(string baseUrl, string authToken)
            : base(baseUrl, authToken)
        {
        }

        public async Task<ApiResponse<User>> Authenticate_JwtToken(string username, string password)
        {
            string endpoint = "Account/Authenticate";
            User user = new User();
            user.Username = username;
            user.Password = password;
            string content = JsonConvert.SerializeObject(user);
            ApiResponse<User> response = await Post<User>(endpoint, content);
            return response;
        }

        public async Task<ApiResponse<User>> Create(User user)
        {
            string endpoint = $"Account/Create";
            string content = JsonConvert.SerializeObject(endpoint);


            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> Delete(int userId)
        {
            string endpoint = $"Account/Delete/{userId}";
            ApiResponse<bool> response = await Delete(endpoint);
            return response;
        }

        public async Task<ApiResponse<List<User>>> GetAll(string filter = "")
        {
            string endpoint = $"Account/GetAll";
            //TODO: filter.
            ApiResponse<List<User>> response = await Get<List<User>>(endpoint);
            return response;
        }

        public async Task<ApiResponse<User>> GetById(int userId)
        {
            string endpoint = $"Account/GetById/{userId}";
            ApiResponse<User> response = await Get<User>(endpoint);
            return response;
        }

        public async Task<ApiResponse<User>> GetByUsername(string username)
        {
            string endpoint = $"Account/GetByUsername/{username}";
            ApiResponse<User> response = await Get<User>(endpoint);
            return response;
        }

        public async Task<ApiResponse<List<Role>>> GetApplicationRoles(string roleName = "")
        {
            string endpoint =  $"Account/GetRoles";
            string content = "";

            throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<UserRole>>> GetUserRoles(int userId, string roleName = "")
        {
            string endpoint = $"Account/GetUserRoles/{userId}/{roleName}";
            string content = "";

            ApiResponse<List<UserRole>> response = await Post<List<UserRole>>(endpoint, content);

            return response;
        }

        public async void Update(User user)
        {
            string endpoint = $"Account/Update";
            string content = JsonConvert.SerializeObject(user);

            await Put<User>(endpoint, content);
        }
    }
}

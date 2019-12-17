using DataAccessLayer.Entities.Account;
using DataAccessLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Services
{
    public class AccountDalService : DalServiceBase, IAccountDalService
    {
        public AccountDalService(string connectionString) : base(connectionString)
        {
        }

        public async Task<User> spUserInsert(User user)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]{
                new SqlParameter("@firstName", user.FirstName),
                new SqlParameter("@lastName", user.LastName),
                new SqlParameter("@passwordHash", user.PasswordHash),
                new SqlParameter("@passwordSalt", user.PasswordSalt),
                new SqlParameter("@username", user.Username),
                new SqlParameter("@email", user.Email)
            };
            db.ExecuteNonQueryAsync("account.spUserInsert", sqlParameters);

            return await spUserGetByUsername(user.Username);
        }

        public async Task<User> spUserUpdate(User user)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]{
                new SqlParameter("@userId", user.UserId),//For identifying which user to update.
                new SqlParameter("@firstName", user.FirstName),
                new SqlParameter("@lastName", user.LastName),
                new SqlParameter("@passwordHash", user.PasswordHash),
                new SqlParameter("@passwordSalt", user.PasswordSalt),
                //new SqlParameter("@username", user.Username),//Not allowing users to change this.
                //new SqlParameter("@email", user.Email),//Not allowing users to change this.
                new SqlParameter("@isActive", DBNull.Value),
            };
            db.ExecuteNonQueryAsync("account.spUserUpdate", sqlParameters);

            return await spUserGetById(user.UserId);
        }

        /// <summary>
        /// Note, pw hashes should not be returned here.
        /// </summary>
        /// <returns></returns>
        public async Task<List<User>> spUsersGetAll(string filter)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]{
                new SqlParameter("@filter", filter)
            };
            return await db.ExecuteReaderAsync<User>("account.spUsersGetAll", sqlParameters);
        }

        public async Task<User> spUserGetById(int id)
        {
            //@userId INT = NULL
            //, @username VARCHAR(32) = NULL
            //, @token VARCHAR(MAX) = NULL

            SqlParameter[] sqlParameters = new SqlParameter[]{
                new SqlParameter("@userId", id)
            };
            return (await db.ExecuteReaderAsync<User>("account.spUserGet", sqlParameters))?.FirstOrDefault();
        }

        public async Task<User> spUserGetByUsername(string username)
        {
            //@userId INT = NULL
            //, @username VARCHAR(32) = NULL
            //, @token VARCHAR(MAX) = NULL

            SqlParameter[] sqlParameters = new SqlParameter[]{
                new SqlParameter("@userId", DBNull.Value),
                new SqlParameter("@username", username)
            };
            return (await db.ExecuteReaderAsync<User>("account.spUserGet", sqlParameters))?.FirstOrDefault();
        }

        public async Task<User> spUserGetByAccessToken(string token)
        {
            //@userId INT = NULL
            //, @username VARCHAR(32) = NULL
            //, @token VARCHAR(MAX) = NULL

            SqlParameter[] sqlParameters = new SqlParameter[]{
                new SqlParameter("@userId", DBNull.Value),
                new SqlParameter("@username", DBNull.Value),
                new SqlParameter("@token", token)
            };
            return (await db.ExecuteReaderAsync<User>("account.spUserGet", sqlParameters))?.FirstOrDefault();
        }

        public async Task<bool> spUserActivateById(int id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]{
                new SqlParameter("@userId", id),//For identifying which user to update.
                new SqlParameter("@firstName", DBNull.Value),
                new SqlParameter("@lastName", DBNull.Value),
                new SqlParameter("@passwordHash", DBNull.Value),
                new SqlParameter("@passwordSalt", DBNull.Value),
                //new SqlParameter("@username", user.Username),//Not allowing users to change this.
                //new SqlParameter("@email", user.Email),//Not allowing users to change this.
                new SqlParameter("@isActive", 0b0001),
            };
            db.ExecuteNonQueryAsync("account.spUserUpdate", sqlParameters);

            return (await spUserGetById(id))?.UserId == id;//Assuming that if the user exists it was updated.
        }

        public async Task<bool> spUserDeactivateById(int id)
        {
            SqlParameter[] sqlParameters = new SqlParameter[]{
                new SqlParameter("@userId", id),//For identifying which user to update.
                new SqlParameter("@firstName", DBNull.Value),
                new SqlParameter("@lastName", DBNull.Value),
                new SqlParameter("@passwordHash", DBNull.Value),
                new SqlParameter("@passwordSalt", DBNull.Value),
                //new SqlParameter("@username", user.Username),//Not allowing users to change this.
                //new SqlParameter("@email", user.Email),//Not allowing users to change this.
                new SqlParameter("@isActive", 0b0000),
            };
            db.ExecuteNonQueryAsync("account.spUserUpdate", sqlParameters);

            return (await spUserGetById(id))?.UserId == id;//Assuming that if the user exists it was updated.
        }

        public async Task<List<UserRole>> spUserRolesGet(int id, string appName = "", string roleName = "")
        {
            //[account].[spUserRolesGet]
            //@userId INT
            //, @appName  VARCHAR(32) = ''
            //, @roleName VARCHAR(32) = ''

            SqlParameter[] sqlParameters = new SqlParameter[]{
                new SqlParameter("@userId", id),
                new SqlParameter("@appName", appName),
                new SqlParameter("@roleName", roleName)
            };
            return await db.ExecuteReaderAsync<UserRole>("account.spUserRolesGet", sqlParameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="appName"></param>
        /// <returns>The token if it exists. Null otherwise.</returns>
        public async Task<UserToken> spUserTokenGet(int id, string appName)
        {
            //[account].[spUserTokenGet]
            //@userId INT
            //, @appName  VARCHAR(32)

            SqlParameter[] sqlParameters = new SqlParameter[]{
                new SqlParameter("@userId", id),
                new SqlParameter("@appName", appName)
            };
            return (await db.ExecuteReaderAsync<UserToken>("account.spUserTokenGet", sqlParameters))?.FirstOrDefault();
        }
    }
}

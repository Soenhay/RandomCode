using DataAccessLayer.Entities.Account;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Interface
{
    public interface IAccountDalService
    {
        #region User
        Task<User> spUserInsert(User user);
        Task<User> spUserUpdate(User user);
        Task<List<User>> spUsersGetAll(string filter);
        Task<User> spUserGetById(int id);
        Task<User> spUserGetByUsername(string username);
        Task<User> spUserGetByAccessToken(string token);
        /// <summary>
        /// Calls spUserUpdate
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> spUserDeactivateById(int id);
        Task<List<UserRole>> spUserRolesGet(int id, string appName = "", string roleName = "");
        Task<UserToken> spUserTokenGet(int id, string appName);
        #endregion
    }
}

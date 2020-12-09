using Common.Models;
using StarWeb.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace MyApp.Services
{
    //asp net core authentication without entity framework
    //https://markjohnson.io/articles/asp-net-core-identity-without-entity-framework/

    //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-custom-storage-providers?view=aspnetcore-2.2
    //IUserRoleStore
    //IUserClaimStore
    //IUserPasswordStore
    //IUserSecurityStampStore
    //IUserEmailStore
    //IUserPhoneNumberStore
    //IQueryableUserStore
    //IUserLoginStore
    //IUserTwoFactorStore
    //IUserLockoutStore

    public class UserStore : IUserStore<UserModel>, IUserEmailStore<UserModel>, IUserPasswordStore<UserModel>, IUserRoleStore<UserModel>, IUserClaimStore<UserModel>
    {
        private AppSettings _settings;
        private IApplicationRoleService _applicationRoleService;
        private IApplicationUserService _applicationUserService;

        public UserStore(
            IOptions<AppSettings> settings,
            IApplicationRoleService applicationRoleService,
            IApplicationUserService applicationUserService)
        {
            _settings = settings.Value;
            _applicationRoleService = applicationRoleService;
            _applicationUserService = applicationUserService;
        }

        #region IUserStore

        public void Dispose()
        {
            // Nothing to dispose.
        }

        public async Task<IdentityResult> CreateAsync(UserModel user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bllResult = await _applicationUserService.UserCreate(user);

            if (bllResult.success)
            {
                return IdentityResult.Success;
            }
            else
            {
                return IdentityResult.Failed();
            }
        }

        public async Task<IdentityResult> DeleteAsync(UserModel user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await _applicationUserService.UserDeleteById(user.UserId))
            {
                return IdentityResult.Success;
            }
            else
            {
                return IdentityResult.Failed();
            }
        }

        public async Task<UserModel> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            UserModel user = await _applicationUserService.UserGetById(Convert.ToInt32(userId));
            return user;
        }

        public async Task<UserModel> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            UserModel user = await _applicationUserService.UserGetByUsername(normalizedUserName);
            return user;
        }

        public Task<string> GetNormalizedUserNameAsync(UserModel user, CancellationToken cancellationToken)
        {
            //Doesn't really need to be normalized b/c the DB is case insensitive.
            return Task.FromResult(user.Username);
        }

        public Task<string> GetUserIdAsync(UserModel user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserId.ToString());
        }

        public Task<string> GetUserNameAsync(UserModel user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Username);
        }

        public Task SetNormalizedUserNameAsync(UserModel user, string normalizedName, CancellationToken cancellationToken)
        {
            //Doesn't really need to be normalized b/c the DB is case insensitive.
            return Task.FromResult(0);
        }

        public async Task SetUserNameAsync(UserModel user, string userName, CancellationToken cancellationToken)
        {
            if (await _applicationUserService.UserGetByUsername(userName) == null)
            {
                user.Username = userName;
            }
            else
            {
                throw new InvalidOperationException("Username is already in use.");
            }
        }

        public async Task<IdentityResult> UpdateAsync(UserModel user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            UserModel userUpdated = await _applicationUserService.UserUpdate(user);
            if (userUpdated != null)
            {
                return IdentityResult.Success;
            }
            else
            {
                return IdentityResult.Failed();
            }
        }
        #endregion

        #region IUserEmailStore

        public Task SetEmailAsync(UserModel user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(UserModel user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(UserModel user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public async Task SetEmailConfirmedAsync(UserModel user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            await _applicationUserService.UserUpdate(user);
        }

        public async Task<UserModel> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //UserModel userFound = await _applicationUserService.UserGetByEmail(normalizedEmail);
            //return userFound;

            return null;
        }

        #endregion

        #region IUserPasswordStore

        public Task<string> GetNormalizedEmailAsync(UserModel user, CancellationToken cancellationToken)
        {
            //Doesn't really need to be normalized b/c the DB is case insensitive.
            return Task.FromResult(user.Email);
        }

        public Task SetNormalizedEmailAsync(UserModel user, string normalizedEmail, CancellationToken cancellationToken)
        {
            //Doesn't really need to be normalized b/c the DB is case insensitive.
            user.Email = normalizedEmail;
            return Task.FromResult(0);
        }

        public Task SetPasswordHashAsync(UserModel user, string passwordHash, CancellationToken cancellationToken)
        {
            //userManager.ResetPasswordAsync calls this function with a new passwordHash.
            //raw(unhashed) Password should be set to the model prior to calling this.
            //Not using passwordHash since our API is creating the hash.
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(UserModel user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Password);
        }

        public Task<bool> HasPasswordAsync(UserModel user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!String.IsNullOrEmpty(user.Password));
        }

        #endregion

        #region IUserRoleStore

        public async Task AddToRoleAsync(UserModel user, string roleName, CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            var appRoles = await _applicationRoleService.RolesGetAll();
            var appRole = appRoles.Find(m => m.roleName.Equals(roleName, StringComparison.InvariantCultureIgnoreCase) && m.applicationName.Equals(_settings.AppNameLeft, StringComparison.InvariantCultureIgnoreCase));
            if (appRole != null)
            {
                await _applicationUserService.UserRoleAssign(user.UserId, appRole.applicationId, appRole.roleId);
            }
        }

        public async Task RemoveFromRoleAsync(UserModel user, string roleName, CancellationToken cancellationToken)
        {
            //throw new NotImplementedException();
            var userRoles = await _applicationUserService.UserGetRoles(user.UserId, _settings.AppNameLeft, roleName);
            if (userRoles.Count > 0)
            {
                await _applicationUserService.UserRoleUnassign(user.UserId, userRoles[0].applicationId, userRoles[0].roleId);
            }
        }

        public async Task<IList<string>> GetRolesAsync(UserModel user, CancellationToken cancellationToken)
        {
            var userRoles = await _applicationUserService.UserGetRoles(user.UserId, _settings.AppNameLeft);
            return userRoles?.FindAll(m => m.applicationName.Equals(_settings.AppNameLeft, StringComparison.InvariantCultureIgnoreCase))
                .Select(m => m.roleName)?.ToList() ?? new List<string>();
        }

        public async Task<bool> IsInRoleAsync(UserModel user, string roleName, CancellationToken cancellationToken)
        {
            var userRoles = await _applicationUserService.UserGetRoles(user.UserId, _settings.AppNameLeft, roleName);
            return (userRoles.Find(m => m.roleName.Equals(roleName, StringComparison.InvariantCultureIgnoreCase) && m.applicationName.Equals(_settings.AppNameLeft, StringComparison.InvariantCultureIgnoreCase)) != null);
        }

        public Task<IList<UserModel>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IUserClaimStore

        public async Task<IList<Claim>> GetClaimsAsync(UserModel user, CancellationToken cancellationToken)
        {
            string issuer = _settings.AuthenticationOptions.issuer;
            var claims = new List<Claim> {
                            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString(), null, issuer)
                           , new Claim(ClaimTypes.Name, user.Username, null, issuer)
                           , new Claim(MyClaimTypes.AppName, _settings.AppNameLeft, null, issuer)
                           , new Claim(MyClaimTypes.ApiToken, user.Token, null, issuer)
                    };
            var userRoles = await GetRolesAsync(user, cancellationToken);
            foreach (string roleName in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName, null, issuer));
            }
            return claims;
        }

        public Task AddClaimsAsync(UserModel user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task ReplaceClaimAsync(UserModel user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimsAsync(UserModel user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IList<UserModel>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

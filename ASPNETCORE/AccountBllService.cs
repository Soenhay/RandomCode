using AutoMapper;
using Common.Helpers;
using DataAccessLayer.Entities.Account;
using DataAccessLayer.Interface;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApi.Dtos;
using WebApi.Dtos.Account;
using WebApi.Helpers;

namespace WebApi.Services
{
    public interface IAccountBllService
    {
        Task<UserDto> UserAuthenticateGetJwtToken(ControllerInfo ci, string username, string password);
        Task<UserDto> UserAuthenticateGetAccessToken(ControllerInfo ci, string username, string password);
        Task<UserDto> UserAuthenticateByAccessToken(ControllerInfo ci, string accessToken);
        Task<List<UserDto>> UsersGetAll(string filter);
        Task<UserDto> UserGetById(int id);
        Task<UserDto> UserGetByUsername(string username);
        Task<bool> UserIsUsernameTaken(string username);
        Task<BllResult<UserDto>> UserCreate(ControllerInfo ci, UserDto userDto);
        Task<BllResult<UserDto>> UserUpdate(ControllerInfo ci, UserDto userDto);
        Task<bool> UserDeleteById(ControllerInfo ci, int id);
        Task<List<UserRoleDto>> UserGetRoles(int id, string appName = "", string roleName = "");
        Task<BllResult<UserTokenDto>> UserTokenGet(int id, string appName);
    }

    /// <summary>
    /// Business Logic Layer (BLL) for Accounts.
    /// </summary>
    public class AccountBllService : IAccountBllService
    {
        private readonly AppSettings _appSettings;
        private IAccountDalService _accountDalService;
        private IMapper _mapper;
        private IApiLogService _apiBllService;

        public AccountBllService(IAccountDalService accountDalService, IApiLogService apiBllService, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _accountDalService = accountDalService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _apiBllService = apiBllService;
        }

        /// <summary>
        /// Authenticate the user and return the UserDto with a token. The UserDto.Token field will be a JwtToken.
        /// </summary>
        /// <param name="ci"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<UserDto> UserAuthenticateGetJwtToken(ControllerInfo ci, string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                doApiLog(ci, ApiLogType.LoginAttempt, $"Failed: missing UN or PW for Username: {username}, [CIP].");
                return null;
            }

            var user = await _accountDalService.spUserGetByUsername(username);

            // check if username exists
            if (user == null || user?.PasswordHash == null || user?.PasswordSalt == null)
            {
                doApiLog(ci, ApiLogType.LoginAttempt, $"Failed: invalid UN for Username: {username}, [CIP].");
                return null;
            }

            // check if password is correct
            if (!CryptographyHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                doApiLog(ci, ApiLogType.LoginAttempt, $"Failed: invalid PW for Username: {username}, [CIP].");
                return null;
            }

            // authentication successful so copy user over.
            var userDto = _mapper.Map<UserDto>(user);
            //Then generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.authenticationOptions.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _appSettings.authenticationOptions.issuer,
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username.ToString()),//, null, _appSettings.jwtOptions.issuer)
                    new Claim("AppName",  ci.AppName)
                }),
                Expires = DateTime.UtcNow.AddHours(_appSettings.authenticationOptions.TokenExpirationHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            userDto.Token = tokenHandler.WriteToken(token);

            //remove password before returning... Not necessary since user entity does not have password field. Just in case...
            userDto.Password = "";

            doApiLog(ci, ApiLogType.LoginAttempt, $"Success: login for Username: {username}, [CIP].");
            return userDto;
        }

        /// <summary>
        /// Authenticate the user and return the UserDto with a token. The UserDto.Token field will be a custom AccessToken.
        /// </summary>
        /// <param name="ci"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<UserDto> UserAuthenticateGetAccessToken(ControllerInfo ci, string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                doApiLog(ci, ApiLogType.LoginAttempt, $"Failed: missing UN or PW for Username: {username}, [CIP].");
                return null;
            }

            var user = await _accountDalService.spUserGetByUsername(username);

            // check if username exists
            if (user == null || user?.PasswordHash == null || user?.PasswordSalt == null)
            {
                doApiLog(ci, ApiLogType.LoginAttempt, $"Failed: invalid UN for Username: {username}, [CIP].");
                return null;
            }

            // check if password is correct
            if (!CryptographyHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                doApiLog(ci, ApiLogType.LoginAttempt, $"Failed: invalid PW for Username: {username}, [CIP].");
                return null;
            }

            // authentication successful so copy user over.
            var userDto = _mapper.Map<UserDto>(user);
            //Then get access token if it exists.

            BllResult<UserTokenDto> userToken = await UserTokenGet(user.UserId, ci.AppName);

            if (userToken == null)
            {
                doApiLog(ci, ApiLogType.LoginAttempt, $"Failed: token does not exist for Username: {username}, [CIP].");
                return null;
            }

            userDto.Token = userToken.data.Token;

            //remove password before returning... Not necessary since user entity does not have password field. Just in case...
            userDto.Password = "";

            doApiLog(ci, ApiLogType.LoginAttempt, $"Success: login for Username: {username}, [CIP].");
            return userDto;
        }

        /// <summary>
        /// Authenticate the user and return the UserDto with a token. The token will likely be the same token passed in.
        /// <para>This is for tokens stored in the DB. Generally we are only using them for internal applications so they do not have to keep logging in.</para>
        /// </summary>
        /// <param name="ci"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public async Task<UserDto> UserAuthenticateByAccessToken(ControllerInfo ci, string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                doApiLog(ci, ApiLogType.LoginAttempt, $"Failed: missing accessToken: [CIP].");
                return null;
            }

            var user = await _accountDalService.spUserGetByAccessToken(accessToken);

            // check if username exists
            if (user == null || user?.PasswordHash == null || user?.PasswordSalt == null)
            {
                doApiLog(ci, ApiLogType.LoginAttempt, $"Failed: invalid accessToken: {accessToken}, [CIP].");
                return null;
            }

            // authentication successful so copy user over.
            var userDto = _mapper.Map<UserDto>(user);

            //TODO:

            doApiLog(ci, ApiLogType.LoginAttempt, $"Success: login for accessToken: {accessToken}, [CIP].");
            return userDto;
        }

        public async Task<List<UserDto>> UsersGetAll(string filter)
        {
            var userList = await _accountDalService.spUsersGetAll(filter);
            var userDtoList = _mapper.Map<List<UserDto>>(userList);
            return userDtoList;
        }

        public async Task<UserDto> UserGetById(int id)
        {
            var user = await _accountDalService.spUserGetById(id);
            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }

        public async Task<UserDto> UserGetByUsername(string username)
        {
            var user = await _accountDalService.spUserGetByUsername(username);
            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }

        public async Task<bool> UserIsUsernameTaken(string username)
        {
            return ((await UserGetByUsername(username))?.Username == username);
        }

        public async Task<BllResult<UserDto>> UserCreate(ControllerInfo ci, UserDto userDto)
        {
            BllResult<UserDto> result = new BllResult<UserDto>();
            result.data = null;
            result.success = false;

            // validation
            if (!Common.Helpers.EmailHelper.IsValidEmail(userDto.Email))
            {
                result.message = "Email is required";
                return result;
            }

            if (string.IsNullOrWhiteSpace(userDto.Password))
            {
                result.message = "Password is required";
                return result;
            }

            if (await UserIsUsernameTaken(userDto.Username))
            {
                result.message = "Username \"" + userDto.Username + "\" is already taken";
                return result;
            }

            //Input appears to be valid.

            var user = _mapper.Map<User>(userDto);

            byte[] passwordHash, passwordSalt;
            CryptographyHelper.CreatePasswordHash(userDto.Password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            user = await _accountDalService.spUserInsert(user);
            userDto = _mapper.Map<UserDto>(user);

            result.message = "success";
            result.success = true;
            result.data = userDto;

            doApiLog(ci, ApiLogType.AccountChange, $"Success: UserCreate: {userDto.Username}, [CUN], [CIP].");
            return result;
        }

        /// <summary>
        /// ID is required.
        /// To not update a specific field set it to null.
        /// </summary>
        /// <param name="ci"></param>
        /// <param name="userDto"></param>
        /// <returns></returns>
        public async Task<BllResult<UserDto>> UserUpdate(ControllerInfo ci, UserDto userDto)
        {
            BllResult<UserDto> result = new BllResult<UserDto>();
            result.data = null;
            result.success = false;

            var user = await _accountDalService.spUserGetById(userDto.UserId);

            if (user == null)
            {
                result.message = "User not found";
                return result;
            }

            if (userDto.Username != user.Username)
            {
                // username has changed so check if the new username is already taken
                if (UserIsUsernameTaken(user.Username).Result)
                {
                    result.message = "Username " + user.Username + " is already taken";
                    return result;
                }
            }

            // update user properties
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            //user.Username = userParam.Username;//Lets not allow changing this?

            // update password if it was entered
            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                byte[] passwordHash, passwordSalt;
                CryptographyHelper.CreatePasswordHash(userDto.Password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            user = await _accountDalService.spUserUpdate(user);
            userDto = _mapper.Map<UserDto>(user);

            result.message = "success";
            result.success = true;
            result.data = userDto;

            doApiLog(ci, ApiLogType.AccountChange, $"Success: UserUpdate: {userDto.Username}, [CUN], [CIP].");
            return result;
        }

        /// <summary>
        /// The user will be deactivated, not deleted.
        /// </summary>
        /// <param name="ci"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> UserDeleteById(ControllerInfo ci, int id)
        {
            return await _accountDalService.spUserDeactivateById(id);
        }

        public async Task<List<UserRoleDto>> UserGetRoles(int id, string appName = "", string roleName = "")
        {
            var userRoles = await _accountDalService.spUserRolesGet(id, appName, roleName);
            var userRolesDto = _mapper.Map<List<UserRoleDto>>(userRoles);

            return userRolesDto;
        }

        /// <summary> 
        /// Get the users active permanent token if it exists.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        public async Task<BllResult<UserTokenDto>> UserTokenGet(int id, string appName)
        {
            BllResult<UserTokenDto> result = new BllResult<UserTokenDto>();
            result.data = null;
            result.success = false;

            var token = await _accountDalService.spUserTokenGet(id, appName);
            var tokenDto = _mapper.Map<UserTokenDto>(token);

            if (tokenDto != null)
            {
                result.data = tokenDto;
                result.success = true;
            }

            return result;
        }

        private void doApiLog(ControllerInfo ci, string type, string message, string exception = "")
        {
            message = message.Replace("[CUN]", $"CallerUN: {ci.clientUsername}").Replace("[CIP]", $"CallerIP: {ci.clientIP}");
            _apiBllService.AddLog(type, ci.controllerName, ci.actionName, message, exception, ci.clientIP, ci.AppName);
        }
    }
}

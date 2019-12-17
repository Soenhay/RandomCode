using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using WebApi.Authorization;
using WebApi.Dtos;
using WebApi.Dtos.Account;
using WebApi.Helpers;
using WebApi.Services;
using WebApi.Swagger.Examples;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("v1/[controller]")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    public class AccountController : ControllerBase
    {

        private Services.IAccountBllService _accountService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(IAccountBllService accountService, 
            IMapper mapper,
            IOptions<AppSettings> appSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            _accountService = accountService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }
        
        #region User

        /// <summary>
        /// Authenticate the user and get a Jwt token for subsequent requests.
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Authenticate", Name = "AccountAuthenticate_JwtToken")]
        [SwaggerRequestExample(typeof(object), typeof(AccountAuthenticationExample))]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Authenticate([FromBody] UserDto userDto)
        {
            var user = await _accountService.UserAuthenticateGetJwtToken(ControllerContext.GetControllerInfo(), userDto.Username, userDto.Password);

            if (user == null)
            {
                return BadRequest(new ApiErrorResult() { message = "Username or password is incorrect" });
            }

            return Ok(user);
        }

        /// <summary>
        /// Authenticate the user and get an access token for the subsequent requests if it exists.
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Token", Name = "AccountAuthenticate_AccessToken")]
        [SwaggerRequestExample(typeof(object), typeof(AccountAuthenticationExample))]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Token([FromBody] UserDto userDto)
        {
            var user = await _accountService.UserAuthenticateGetAccessToken(ControllerContext.GetControllerInfo(), userDto.Username, userDto.Password);

            if (user == null)
            {
                return BadRequest(new ApiErrorResult() { message = "Username or password is incorrect" });
            }

            return Ok(user);
        }

        /// <summary>
        /// Get a users token if it exists.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Token", Name = "AccountGet_AccessToken")]
        [SwaggerRequestExample(typeof(object), typeof(AccountAuthenticationExample))]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Token()
        {
            var ci = ControllerContext.GetControllerInfo();
            var bllResult = await _accountService.UserTokenGet(ci.clientUserId, ci.AppName);

            if (!bllResult.success)
            {
                return BadRequest(new ApiErrorResult() { message = "Token does not exist" });
            }

            return Ok(bllResult.data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [Authorize(Policy = Role.Admin)]
        [HttpPost("Create", Name = "AccountCreate")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] UserDto userDto)
        {
            var user = await _accountService.UserCreate(ControllerContext.GetControllerInfo(), userDto);

            if (!user.success)
            {
                return BadRequest(new ApiErrorResult() { message = "User could not be created." });
            }

            return Ok(user.data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">The user was deleted.</response>
        /// <response code="401">Unauthorized to delete.</response> 
        /// <response code="404">No user found to delete.</response> 
        [Authorize(Policy = Role.Admin)]
        [HttpDelete("Delete/{id}", Name = "AccountDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _accountService.UserGetById(id);

            if (user == null)
            {
                //No user found to delete.
                return NotFound();
            }
            else
            {
                if (await _accountService.UserDeleteById(ControllerContext.GetControllerInfo(), id))
                {
                    //The user was deleted.
                    return Ok();
                }
                else
                {
                    //Assuming they were unauthorized to delete the user.
                    return this.Unauthorized();
                }
            }
        }

        /// <summary>
        /// --Format example:  '|usernameLike=test|firstNameLike=|lastNameLike=testin|isLocked=0|isActive=1|'
        /// </summary>
        /// <returns></returns>
        /// <response code="400">Bad request. Make sure filter is empty or make sure filter is in the correct format '|key=value|' and try again.</response> 
        [Authorize(Policy = Role.Admin)]
        [HttpGet("GetAll", Name = "AccountGetAll")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll(string filter = "")
        {
            // @filter VARCHAR(128) = ''
            try
            {
                var users = await _accountService.UsersGetAll(filter);
                return Ok(users);
            }
            catch (Exception)
            {
                return BadRequest(new ApiErrorResult() { message = "Bad request. Make sure filter is empty or make sure filter is in the correct format '|key=value|' and try again." });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Returns the requested user.</response>
        /// <response code="404">NotFound.</response> 
        [HttpGet("GetById/{id}", Name = "AccountGetById")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _accountService.UserGetById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet("GetByUsername/{username}", Name = "AccountGetByUsername")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var user = await _accountService.UserGetByUsername(username);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleName">Optional No spaces</param>
        /// <returns></returns>
        [HttpGet("GetUserRoles/{userId}/{roleName?}", Name = "AccountGetUserRoles")]
        [ProducesResponseType(typeof(List<UserRoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserRoles(int userId, string roleName)
        {
            ControllerInfo ci = ControllerContext.GetControllerInfo();
            var userRoles = await _accountService.UserGetRoles(userId, ci.AppName, roleName);
            if (userRoles == null)
            {
                return NotFound();
            }
            return Ok(userRoles);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns></returns>
        [HttpPut("Update", Name = "AccountUpdate")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update([FromBody] UserDto userDto)
        {
            var updatedUser = await _accountService.UserUpdate(ControllerContext.GetControllerInfo(), userDto);

            if (!updatedUser.success)
            {
                return this.Unauthorized();
            }

            return Ok(updatedUser.data);
        }

        #endregion
    }
}

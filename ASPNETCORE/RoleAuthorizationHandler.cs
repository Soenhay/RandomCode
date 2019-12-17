using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using WebApi.Dtos.Account;
using WebApi.Services;

namespace WebApi.Authorization
{
    public class RoleAuthorizationHandler : AuthorizationHandler<RoleAuthorizationRequirement>
    {
        IAccountBllService _accountBllService;

        public RoleAuthorizationHandler(IAccountBllService accountBllService) {
            _accountBllService = accountBllService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       RoleAuthorizationRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier &&
                                            c.Issuer == requirement.Issuer))
            {
                //TODO: Use the following if targeting a ver*sion of
                //.NET Framework older than 4.6:
                //      return Task.FromResult(0);
                return Task.CompletedTask;
            }

            int userId = 0;
            string appName = context.User.FindFirst("AppName")?.Value;

            if (int.TryParse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId))
            {
                List<UserRoleDto> userRoles = _accountBllService.UserGetRoles(userId, appName, requirement.Role).Result;

                if (userRoles.Find(m => m.roleName.Equals(requirement.Role, StringComparison.InvariantCultureIgnoreCase) && m.applicationName.Equals(appName, StringComparison.InvariantCultureIgnoreCase)) != null)
                {
                    context.Succeed(requirement);
                }
            }

            //TODO: Use the following if targeting a version of
            //.NET Framework older than 4.6:
            //      return Task.FromResult(0);
            return Task.CompletedTask;
        }
    }
}

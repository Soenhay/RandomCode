using Microsoft.AspNetCore.Authorization;

namespace WebApi.Authorization
{
    public class RoleAuthorizationRequirement : IAuthorizationRequirement
    {
        public string Issuer { get; }
        public string Role { get; }

        public RoleAuthorizationRequirement(string issuer, string roleName)
        {
            Issuer = issuer;
            Role = roleName;
        }
    }
}

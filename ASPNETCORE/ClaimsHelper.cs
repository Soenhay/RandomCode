using System.Linq;
using System.Security.Claims;

namespace WebApi.Helpers
{
    public static class ClaimsHelper
    {
        /// <summary>
        /// @((ClaimsIdentity) User.Identity).GetSpecificClaim("someclaimtype")
        /// </summary>
        /// <param name="claimsIdentity"></param>
        /// <param name="claimType"></param>
        /// <returns></returns>
        /// <remarks>https://stackoverflow.com/questions/39125347/how-to-get-claim-inside-asp-net-core-razor-view</remarks>
        public static string GetSpecificClaim(this ClaimsIdentity claimsIdentity, string claimType)
        {
            var claim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == claimType);

            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetSpecificClaim(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            var claim = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == claimType);

            return (claim != null) ? claim.Value : string.Empty;
        }
    }
}

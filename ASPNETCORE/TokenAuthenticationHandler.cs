using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using WebApi.Dtos;
using WebApi.Helpers;
using WebApi.Services;

namespace WebApi.Athentication
{
    //https://github.com/ignas-sakalauskas/CustomAuthenticationNetCore20/blob/master/CustomAuthNetCore20/Authentication/CustomAuthHandler.cs
    //https://ignas.me/tech/custom-authentication-asp-net-core-20/
    //https://geeklearning.io/how-to-migrate-your-authentication-middleware-to-asp-net-core-2-0/
    //https://docs.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?view=aspnetcore-3.0


    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        IAccountBllService _accountBllService;
        private readonly AppSettings _appSettings;

        public TokenAuthenticationHandler(IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IAccountBllService accountBllService, IOptions<AppSettings> appSettings)
            : base(options, logger, encoder, clock)
        {
            _accountBllService = accountBllService;
            _appSettings = appSettings.Value;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Get Authorization header value
            if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorization))
            {
                return Task.FromResult(AuthenticateResult.Fail("Cannot read authorization header."));
            }

            ControllerInfo ci = new ControllerInfo();
            ci.controllerName = "";
            ci.actionName = "";
            ci.clientUsername = Request.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value; 
            ci.clientIP = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            ci.AppName = Request.HttpContext.Request.Headers["AppName"];

            var user = _accountBllService.UserAuthenticateByAccessToken(ci, authorization).Result;

            // If auth key from Authorization header is valid then user will not be null.
            // If it was a jwt token then the user will be null and Authentication should be handled in the jwt code.
            // Also, jwt is the default so it shouldn't even call this function if it was already authenticated.
            if (user == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid access token."));
            }
            
            // Create authenticated user
            var identities = new List<ClaimsIdentity> {
                new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString(), null, _appSettings.authenticationOptions.issuer),
                    new Claim(ClaimTypes.Name, user.Username.ToString(), null, _appSettings.authenticationOptions.issuer),
                    new Claim("AppName",  ci.AppName, null, _appSettings.authenticationOptions.issuer)
                },
                TokenAuthenticationOptions.DefaultScheme
                )
            };
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identities), Options.Scheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public static class CustomAuthBuilderExtensions
    {
        // Custom authentication extension method
        public static AuthenticationBuilder AddCustomAuth(this AuthenticationBuilder builder, Action<TokenAuthenticationOptions> configureOptions)
        {
            // Add custom authentication scheme with custom options and custom handler
            return builder.AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(TokenAuthenticationOptions.DefaultScheme, configureOptions);
        }
    }
}

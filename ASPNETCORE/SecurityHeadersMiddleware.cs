using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Middleware
{
    //https://andrewlock.net/adding-default-security-headers-in-asp-net-core/
    //https://www.3pillarglobal.com/insights/short-circuiting-branching-net-core


    public class SecurityHeadersPolicy
    {
        /// <summary>
        /// Add headers to response.
        /// </summary>
        public IDictionary<string, string> SetHeaders { get; }
             = new Dictionary<string, string>();

        /// <summary>
        /// Remove headers from response.
        /// </summary>
        public ISet<string> RemoveHeaders { get; }
            = new HashSet<string>();

        /// <summary>
        /// Require headers from request
        /// </summary>
        public ISet<string> RequiredHeaders { get; }
            = new HashSet<string>();
    }

    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SecurityHeadersPolicy _policy;

        public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersPolicy policy)
        {
            _next = next;
            _policy = policy;
        }

        public async Task Invoke(HttpContext context)
        {
            IHeaderDictionary headers = context.Response.Headers;

            foreach (var headerValuePair in _policy.SetHeaders)
            {
                headers[headerValuePair.Key] = headerValuePair.Value;
            }

            foreach (var header in _policy.RemoveHeaders)
            {
                headers.Remove(header);
            }

            foreach (string requiredHeader in _policy.RequiredHeaders)
            {
                if (!context.Request.Headers.ContainsKey(requiredHeader))
                {
                    //If a required header is missing then return a message letting the user know.
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync($"Missing required header: {requiredHeader}");
                    return;//Short ciruit the request.
                }
            }

            await _next(context);//Continue down the pipeline.
        }
    }

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeadersMiddleware(this IApplicationBuilder app, SecurityHeadersBuilder builder)
        {
            SecurityHeadersPolicy policy = builder.Build();
            return app.UseMiddleware<SecurityHeadersMiddleware>(policy);
        }
    }
    public class SecurityHeadersBuilder
    {
        private readonly SecurityHeadersPolicy _policy = new SecurityHeadersPolicy();

        public SecurityHeadersBuilder AddDefaultSecurePolicy()
        {
            AddFrameOptionsDeny();
            AddXssProtectionBlock();
            AddContentTypeOptionsNoSniff();
            //AddStrictTransportSecurityMaxAge();
            RemoveServerHeader();
            AddRequiredHeader("AppName");

            return this;
        }

        /// <summary>
        /// This is actually handled by  app.UseHsts();
        /// https://github.com/OWASP/CheatSheetSeries/blob/master/cheatsheets/HTTP_Strict_Transport_Security_Cheat_Sheet.md
        /// </summary>
        /// <param name="maxAgeSeconds">The time, in seconds, that the browser should remember that a site is only to be accessed using HTTPS.</param>
        /// <param name="includeSubDomains">If this optional parameter is specified, this rule applies to all of the site's subdomains as well.</param>
        /// <param name="preload">See Preloading Strict Transport Security for details. Not part of the specification.</param>
        /// <returns></returns>
        public SecurityHeadersBuilder AddStrictTransportSecurityMaxAge(int maxAgeSeconds, bool? includeSubDomains = null, bool? preload = null)
        {
            //Strict-Transport-Security: max-age=<expire-time>
            //Strict-Transport-Security: max-age=<expire-time>; includeSubDomains
            //Strict-Transport-Security: max-age=<expire-time>; preload

            string sd = (bool)includeSubDomains ? " includeSubDomains;" : "";
            string p = (bool)preload ? " preload;" : "";
            _policy.SetHeaders[HeaderNames.StrictTransportSecurity] = $"max-age={maxAgeSeconds}{sd}{p}";
            return this;
        }

        public SecurityHeadersBuilder AddContentTypeOptionsNoSniff()
        {
            _policy.SetHeaders["X-Content-Type-Options"] = "nosniff";
            return this;
        }

        public SecurityHeadersBuilder AddFrameOptionsDeny()
        {
            _policy.SetHeaders["X-Frame-Options"] = "DENY";
            return this;
        }

        public SecurityHeadersBuilder AddFrameOptionsSameOrigin()
        {
            _policy.SetHeaders["X-Frame-Options"] = "SAMEORIGIN";
            return this;
        }

        public SecurityHeadersBuilder AddFrameOptionsSameOrigin(string uri)
        {
            _policy.SetHeaders["X-Frame-Options"] = $"ALLOW-FROM {uri}";
            return this;
        }

        public SecurityHeadersBuilder AddXssProtectionBlock()
        {
            _policy.SetHeaders["X-XSS-Protection"] = "1";
            return this;
        }

        /// <summary>
        /// Could also be removed in ASP.NetCore with :
        /// <para>WebHost.CreateDefaultBuilder(args)             </para>
        /// <para>   .UseKestrel(c => c.AddServerHeader = false) </para>
        /// <para>   .UseStartup&lt;Startup&gt;()                    </para>
        /// <para>   .Build();                                   </para>
        /// </summary>
        /// <returns></returns>
        public SecurityHeadersBuilder RemoveServerHeader()
        {
            _policy.RemoveHeaders.Add("Server");
            return this;
        }

        public SecurityHeadersBuilder AddCustomHeader(string header, string value)
        {
            _policy.SetHeaders[header] = value;
            return this;
        }

        public SecurityHeadersBuilder RemoveHeader(string header)
        {
            _policy.RemoveHeaders.Add(header);
            return this;
        }

        public SecurityHeadersBuilder AddRequiredHeader(string header)
        {
            _policy.RequiredHeaders.Add(header);
            return this;
        }

        public SecurityHeadersPolicy Build()
        {
            return _policy;
        }
    }

}

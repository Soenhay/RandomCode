using Common.Models;
using MyWeb.Authentication;
using MyWeb.Authorization;
using MyWeb.Mappings;
using MyWeb.Middleware;
using MyWeb.Models.Account;
using MyWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting; //For IWebHostEnvironment.IsDevelopment(), etc.
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using RazorHtmlEmails.RazorClassLib.Services;
using System;
using System.Collections.Generic;

namespace MyWeb
{
    public class Startup
    {
        /*
         * Notes:
         * Edit and continue add COMPLUS_ForceENC = 1 to the environment variables.
         * https://github.com/aspnet/AspNetCore/issues/7390
         * 
         */

        public IWebHostEnvironment WebHostEnvironment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            WebHostEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            //.AddJsonOptions(options =>
            //{
            //    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            //    options.JsonSerializerOptions.WriteIndented = true;
            //});
            services.AddRazorPages();
            services.AddCors();
            //services.AddMvc(options => options.Filters.Add(new AuthorizeFilter()))
            services.AddAutoMapperProfile();
            services.Configure<RouteOptions>(options => options.AppendTrailingSlash = true);
            var appSettingsSection = Configuration.GetSection("AppSettings");
            var appSettings = appSettingsSection.Get<AppSettings>();
            var mqttSettings = Configuration.GetSection("MQTTSettings").Get<MQTTSettings>();
            if( WebHostEnvironment.IsDevelopment()){
                mqttSettings.ClientSettings.Id = System.Guid.NewGuid().ToString();
            }
            //string apiBaseUrlAccount = appSettings.MyWebApi.BaseUrlAccount;
            //string apiBaseUrlEmail = appSettings.MyWebApi.BaseUrlEmail;
            //string apiBaseUrlLogging = appSettings.MyWebApi.BaseUrlLogging;
            services.Configure<AppSettings>(appSettingsSection);

            //Explicitly register the settings object by delegating to the IOptions object so that it can be accessed globally via AppServicesHelper.
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptionsMonitor<AppSettings>>().CurrentValue);

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            });
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromMinutes(15);
            });

            //services.AddDefaultIdentity<IdentityUser>().AddRoles<IdentityRole>();
            //services.AddIdentity<UserModel, RoleModel>().AddUserStore<UserModel>().AddDefaultTokenProviders();
            services.AddIdentityCore<UserModel>(options =>
            {
                //Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // Default Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddRoles<RoleModel>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            //services.Configure<IdentityOptions>(options =>
            //{
            //    // Default Lockout settings.
            //    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            //    options.Lockout.MaxFailedAccessAttempts = 5;
            //    options.Lockout.AllowedForNewUsers = true;
            //});

            // configure cookie authentication.
            //var key = Encoding.ASCII.GetBytes(appSettings.AuthenticationOptions.Secret);
            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            services.AddAuthentication(options =>
            {
                //x.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //x.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //x.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //x.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
            })
             .AddOpenIdConnect(options =>
             {
                 options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                 options.Authority = appSettings.OpenIDConnect.authority;
                 options.ResponseType = OpenIdConnectResponseType.Code;
                 options.UsePkce = false;
                 //options.Scope.Clear();
                 //options.Scope.Add("openid");
                 //options.Scope.Add("profile");
                 //options.Scope.Add("email");
                 options.SaveTokens = true;
                 // MetadataAddress represents the Active Directory instance used to authenticate users.
                 options.MetadataAddress = appSettings.OpenIDConnect.metaData;
                 options.ClientId = appSettings.OpenIDConnect.clientId;
                 //options.CallbackPath = "/Account/signin-oidc";

                 options.EventsType = typeof(CustomOidcAuthenticationEvents);
             })
            .AddCookie(options =>
            {
                //Authentication cookie policy.
                options.Cookie.Name = "MyWeb.Auth";
                options.ClaimsIssuer = appSettings.AuthenticationOptions.issuer;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = WebHostEnvironment.IsDevelopment()
                  ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.AccessDeniedPath = "/Account/Denied";
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                //options.EventsType = typeof(CustomCookieAuthenticationEvents);//If we want to overload cookie events. Also need to put this services.AddScoped<CustomCookieAuthenticationEvents>();
            });
            //.AddCookie("AuthenticationTypes.Federation", options =>
            //{
            //});
            //.AddIdentityCookies(options => { });

            //Global cookie policy
            services.Configure<CookiePolicyOptions>(options =>
            {
                //https://www.red-gate.com/simple-talk/dotnet/net-development/using-auth-cookies-in-asp-net-core/
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.HttpOnly = HttpOnlyPolicy.None;
                options.Secure = WebHostEnvironment.IsDevelopment()
                  ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
            });
            //services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/Login");

            //Configure authorization policies
            services.AddSingleton<IAuthorizationHandler, RoleAuthorizationHandler>();
            services.AddAuthorizationCore(x =>
            {
                //var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                //        JwtBearerDefaults.AuthenticationScheme);
                //defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();

                x.AddPolicy(Role.Admin, p => p.Requirements.Add(new RoleAuthorizationRequirement(appSettings.AuthenticationOptions.issuer, Role.Admin)));
                //x.AddPolicy(Role.Admin, p => p.Requirements.Add(new RoleAuthorizationRequirement(OidcConstants.issuer, Role.Admin)));
                x.AddPolicy(Role.SuperUser, p => p.Requirements.Add(new RoleAuthorizationRequirement(appSettings.AuthenticationOptions.issuer, Role.SuperUser)));
                x.AddPolicy(Role.CustomApp, p => p.Requirements.Add(new RoleAuthorizationRequirement(appSettings.AuthenticationOptions.issuer, Role.Admin, Role.CustomApp)));
            });

            //Don't do this b/c then new account email confirmation tokens will also expire... leave it at the default of 24hrs.
            //Change all data Tokens to expire after 3 hours. https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-3.1&tabs=visual-studio#change-all-data-protection-token-lifespans
            //This only affects tokens generated by the userManager and maybe the signInManager. Not the tokens we keep in our DB.
            //services.Configure<DataProtectionTokenProviderOptions>(options =>
            //    options.TokenLifespan = TimeSpan.FromHours(3)
            //    );


            //Additional services.
            services.AddHttpContextAccessor();
            services.AddAccountService();
            services.AddCustomAppReportService();
            services.AddApplicationUserService();
            services.AddApplicationRoleService();
            services.AddEmailService();
            services.AddApiLoggingService();
            services.AddAtmosphereService(mqttSettings);
            services.AddTransient<ICustomAppService, CustomAppService>();
            services.AddTransient<IUserStore<UserModel>, UserStore>();
            services.AddTransient<IRoleStore<RoleModel>, RoleStore>();
            services.AddTransient<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            //services.AddScoped<CustomCookieAuthenticationEvents>();
            services.AddScoped<CustomOidcAuthenticationEvents>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IOptions<AppSettings> settings,
            ILoggerFactory logFactory,
            IHttpContextAccessor accessor)
        {
            //Static services
            lib.ApplicationLogging.LoggerFactory = logFactory;
            AppServicesHelper.Services = app.ApplicationServices;

            //AppSettings appSettings = settings.Value;

            //See Middleware order: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-3.1#middleware-order

            if (WebHostEnvironment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseCookiePolicy(); //The cookie policy middleware is order sensitive. 

            app.UseRouting();
            // app.UseRequestLocalization();
            // app.UseCors();

            //app.ConfigureCustomExceptionMiddleware();

            app.UseSession();//Needs to be after UseRouting and before UseEndpoints. Needs to be before UseAuthentication and UseHttpContextItemsMiddleware since it needs the session.
            app.UseAuthentication();//By invoking the authentication middleware, you will get a HttpContext.User property.
            app.UseHttpContextItemsMiddleware();//Needs to be after UseAuthentication to have the cookie claims loaded and before UseAuthorization so the session can be used.
            app.UseAuthorization();

            ////If the user is not authenticated dont allow access to the folders that startwith the options below
            app.UseStaticFilesMiddleware(options =>
            {
                options.AddAuthSegment("/CustomApp");
                options.AddAuthSegment("/js/CustomApp");
                options.AddFileExtensionContentTypeMapping(".glb", "model/gltf-binary");
                options.AddFileExtensionContentTypeMapping(".webmanifest", "application/manifest+json");
                options.AddFileExtensionContentTypeMapping(".vue", "application/javascript");
            });
                
            app.UseRememberMeCookieSessionMiddleware(options =>
            {
                options.LoginPath = "/Account/Login";
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Default}/{action=Index}/{id?}");
                //endpoints.MapAreaControllerRoute(
                //  "Admin",
                //  "Admin",
                //  "Admin/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
                //endpoints.MapRazorPages();
               
                //endpoints.MapControllerRoute(
                //    "default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

using DataAccessLayer.Interface;
using DataAccessLayer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using WebApi.Athentication;
using WebApi.Authorization;
using WebApi.Helpers;
using WebApi.Mappings;
using WebApi.Middleware;
using WebApi.Services;
using WebApi.Swagger.Examples;
using WebApi.Swagger.Headers;

namespace WebApi
{
    public class Startup
    {
        private readonly ILogger _logger;
        private const string RoutePrefix = "docs";
        private const string RouteTemplate = "/{documentName}/swagger.json";
        private const string ApiDocumentName_v1 = "apiDoc_v1";

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            _logger = logger;

            //Test the logging functions.
            _logger.LogCritical("Test LogCritical");
            _logger.LogError("Test LogError");
            _logger.LogWarning("Test LogWarning");
            _logger.LogInformation("Test LogInformation");
            _logger.LogDebug("Test LogDebug");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddAutoMapperProfile();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(ApiDocumentName_v1, new OpenApiInfo { Title = "MyWebApi", Version = "v1" });
                c.ExampleFilters();
                c.OperationFilter<AddAppNameRequiredHeaderParameter>();

                //https://stackoverflow.com/questions/56234504/migrating-to-swashbuckle-aspnetcore-version-5
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddSwaggerExamplesFromAssemblyOf<AccountAuthenticationExample>();

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            string dbConnectionString = Configuration.GetConnectionString("DbConnection");

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.authenticationOptions.Secret);
            var jwtIssuer = appSettings.authenticationOptions.issuer;
            services.AddAuthentication(x =>
            {
               // x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                //x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultAuthenticateScheme = TokenAuthenticationOptions.DefaultScheme + "," + JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = TokenAuthenticationOptions.DefaultScheme + "," + JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCustomAuth(options =>
            {
                // Configure password for authentication
                options.AuthKey = appSettings.authenticationOptions.Secret;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.ClaimsIssuer = jwtIssuer;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = false,
                    ValidateLifetime = true
                };
            });

            //Configure authorization policies
            services.AddSingleton<IAuthorizationHandler, RoleAuthorizationHandler>();
            services.AddAuthorization(x =>
            {
                //Support both Jwt and our custom Token options.
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                        JwtBearerDefaults.AuthenticationScheme,
                        TokenAuthenticationOptions.DefaultScheme);
                defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                x.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();

                x.AddPolicy(Role.Admin, p => p.Requirements.Add(new RoleAuthorizationRequirement(jwtIssuer, Role.Admin)));
                x.AddPolicy(Role.RadarReport, p => p.Requirements.Add(new RoleAuthorizationRequirement(jwtIssuer, Role.RadarReport)));
                x.AddPolicy(Role.RadarInsert, p => p.Requirements.Add(new RoleAuthorizationRequirement(jwtIssuer, Role.RadarInsert)));
                x.AddPolicy(Role.SuperUser, p => p.Requirements.Add(new RoleAuthorizationRequirement(jwtIssuer, Role.SuperUser)));
            });

            //Additional services.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IAccountDalService, AccountDalService>(s => new AccountDalService(dbConnectionString));
            services.AddSingleton<IAccountBllService, AccountBllService>();
            services.AddSingleton<IApiDalService, ApiDalService>(s => new ApiDalService(dbConnectionString));
            services.AddSingleton<IApiLogService, ApiLogService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                _logger.LogInformation("In Development environment");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                _logger.LogInformation("In Production environment");

                //app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                //app.UseXContentTypeOptions();
                //app.UseReferrerPolicy(opts => opts.NoReferrer());
                //app.UseXXssProtection(options => options.EnabledWithBlockMode());
                //app.UseXfo(options => options.Deny());
            }


            //Swagger
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger((c =>
            {
                c.RouteTemplate = RoutePrefix + RouteTemplate;
            }));
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = RoutePrefix;
                //  http://localhost:<port>/swagger/v1/swagger.json
                c.SwaggerEndpoint($"../{RoutePrefix}/{ApiDocumentName_v1}/swagger.json", "Api Doc v1");
            });

            //Middleware happens in order.
            //app.UseMiddleware<RequestResponseLoggingMiddleware>();
            //app.UseMiddleware<LogRequestMiddleware>();
            //app.UseMiddleware<LogResponseMiddleware>();
            app.UseMiddleware<ApiLoggingMiddleware>();

            app.UseSecurityHeadersMiddleware(new SecurityHeadersBuilder()
              .AddDefaultSecurePolicy()
              .AddRequiredHeader("AppName")
            );

            app.UseAuthentication();
            app.UseMvc();//Most things need to be above this.
        }
    }
}

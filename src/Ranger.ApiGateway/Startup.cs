using System.Security.Cryptography.X509Certificates;
using AspNetCoreRateLimit;
using AspNetCoreRateLimit.Redis;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Ranger.ApiGateway.Data;
using Ranger.ApiUtilities;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Monitoring.HealthChecks;
using Ranger.RabbitMQ;
using Ranger.Redis;
using reCAPTCHA.AspNetCore;

namespace Ranger.ApiGateway
{
    public class Startup
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration configuration;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.Environment = environment;
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<OperationCanceledExceptionFilter>();
            })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                })
                .AddRangerFluentValidation<Startup>();

            services.AddRangerApiVersioning();

            services.ConfigureAutoWrapperModelStateResponseFactory();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicyNames.BelongsToProject, policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser().AddRequirements(new BelongsToProjectRequirement());
                });
                options.AddPolicy(AuthorizationPolicyNames.ValidBreadcrumbApiKey, policyBuilder =>
                {
                    policyBuilder.AddRequirements(new ValidBreadcrumbApiKeyRequirement());
                });
                options.AddPolicy(AuthorizationPolicyNames.ValidProjectApiKey, policyBuilder =>
                {
                    policyBuilder.AddRequirements(new ValidProjectApiKeyRequirement());
                });
                options.AddPolicy(AuthorizationPolicyNames.TenantIdResolved, policyBuilder =>
                {
                    policyBuilder.AddRequirements(new TenantIdResolvedRequirement());
                });
            });

            var identityAuthority = configuration["httpClient:identityAuthority"];
            services.AddPollyPolicyRegistry();
            services.AddIdentityHttpClient("http://identity:5000", identityAuthority, "IdentityServerApi", "89pCcXHuDYTXY");
            services.AddTenantsHttpClient("http://tenants:8082", identityAuthority, "tenantsApi", "cKprgh9wYKWcsm");
            services.AddProjectsHttpClient("http://projects:8086", identityAuthority, "projectsApi", "usGwT8Qsp4La2");
            services.AddSubscriptionsHttpClient("http://subscriptions:8089", identityAuthority, "subscriptionsApi", "4T3SXqXaD6GyGHn4RY");
            services.AddGeofencesHttpClient("http://geofences:8085", identityAuthority, "geofencesApi", "9pwJgpgpu6PNJi");
            services.AddIntegrationsHttpClient("http://integrations:8087", identityAuthority, "integrationsApi", "6HyhzSoSHvxTG");

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IAuthorizationHandler, BelongsToProjectHandler>();
            services.AddScoped<IAuthorizationHandler, ValidBreadcrumbApiKeyHandler>();
            services.AddScoped<IAuthorizationHandler, TenantIdResolvedHandler>();
            services.AddScoped<IAuthorizationHandler, ValidProjectApiKeyHandler>();

            services.AddCors();

            services.AddDbContext<ApiGatewayDbContext>(options =>
            {
                options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            },
                ServiceLifetime.Transient
            );
            services.AddTransient<IApiGatewayDbContextInitializer, ApiGatewayDbContextInitializer>();

            services.AddRedis(configuration["redis:ConnectionString"], out _);

            // Workaround for MAC validation issues on MacOS
            if (configuration.IsIntegrationTesting())
            {
                services.AddDataProtection()
                   .SetApplicationName("ApiGateway")
                   .PersistKeysToDbContext<ApiGatewayDbContext>();
            }
            else
            {
                if (Environment.IsDevelopment())
                {
                    services.AddSwaggerGen("API Gateway", "v1");
                }
                services.AddDataProtection()
                    .SetApplicationName("ApiGateway")
                    .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                    .UnprotectKeysWithAnyCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                    .PersistKeysToDbContext<ApiGatewayDbContext>();
            }

            var recaptchaSettings = configuration.GetOptions<RangerRecaptchaOptions>("recaptchaSettings");
            services.AddRecaptcha(options =>
            {
                options.SecretKey = recaptchaSettings.SecretKey;
                options.SiteKey = recaptchaSettings.SiteKey;
            });

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "apiGateway";

                    options.RequireHttpsMetadata = false;
                });

            services.AddOptions();
            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddStackExchangeRedisStores();

            services.AddLiveHealthCheck();
            services.AddEntityFrameworkHealthCheck<ApiGatewayDbContext>();
            services.AddDockerImageTagHealthCheck();
            services.AddRabbitMQHealthCheck();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddRabbitMqWithOutbox<Startup, ApiGatewayDbContext>();
            builder.RegisterInstance<RangerPusherOptions>(configuration.GetOptions<RangerPusherOptions>("pusher"));
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime)
        {
            if (Environment.IsProduction())
            {
                app.UsePathBase("/api");
            }
            if (Environment.IsDevelopment())
            {
                app.UseSwagger("v1", "API Gateway");
            }
            app.UseCors(builder =>
            {
                builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .WithExposedHeaders("X-Operation")
                    .WithExposedHeaders("X-Pagination-TotalCount")
                    .WithExposedHeaders("X-Pagination-PageCount")
                    .WithExposedHeaders("X-Pagination-Page")
                    .WithExposedHeaders("X-Pagination-SortOrder")
                    .WithExposedHeaders("X-Pagination-OrderBy");
            });
            app.UseAutoWrapper();
            app.UseIpRateLimiting();
            app.UseUnhandedExceptionLogger();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks();
                endpoints.MapLiveTagHealthCheck();
                endpoints.MapEfCoreTagHealthCheck();
                endpoints.MapDockerImageTagHealthCheck();
                endpoints.MapRabbitMQHealthCheck();
            });
        }
    }
}
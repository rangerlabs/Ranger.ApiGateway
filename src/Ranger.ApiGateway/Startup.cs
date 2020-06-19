using System.Security.Cryptography.X509Certificates;
using Autofac;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;

using Ranger.ApiGateway.Data;
using Ranger.ApiUtilities;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.Monitoring.HealthChecks;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    public class Startup
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration configuration;
        private ILoggerFactory loggerFactory;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.Environment = environment;
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
                });
            services.AddAutoWrapper();
            if (Environment.IsDevelopment())
            {
                services.AddSwaggerGen("API Gateway", "v1");
            }

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
                options.AddPolicy(AuthorizationPolicyNames.UserBelongsToProjectOrValidProjectApiKey, policyBuilder =>
                {
                    policyBuilder.AddRequirements(new UserBelongsToProjectOrValidProjectApiKeyRequirement());
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
            services.AddScoped<IAuthorizationHandler, UserBelongsToProjectHandler>();
            services.AddScoped<IAuthorizationHandler, ValidProjectApiKeyHandler>();

            services.AddCors();

            services.AddApiVersioning(o => o.ApiVersionReader = new HeaderApiVersionReader("api-version"));

            services.AddDbContext<ApiGatewayDbContext>(options =>
            {
                options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            },
                ServiceLifetime.Transient
            );
            services.AddTransient<IApiGatewayDbContextInitializer, ApiGatewayDbContextInitializer>();

            services.AddDataProtection()
                .SetApplicationName("ApiGateway")
                .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .UnprotectKeysWithAnyCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .PersistKeysToDbContext<ApiGatewayDbContext>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "apiGateway";

                    options.RequireHttpsMetadata = false;
                });

            services.AddLiveHealthCheck();
            services.AddEntityFrameworkHealthCheck<ApiGatewayDbContext>();
            services.AddDockerImageTagHealthCheck();
            services.AddRabbitMQHealthCheck();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddRabbitMq();
            builder.RegisterInstance<RangerPusherOptions>(configuration.GetOptions<RangerPusherOptions>("pusher"));
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;

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
                   .WithExposedHeaders("X-Operation");
           });
            app.UseAutoWrapper();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapHealthChecks();
                endpoints.MapLiveTagHealthCheck();
                endpoints.MapEfCoreTagHealthCheck();
                endpoints.MapDockerImageTagHealthCheck();
                endpoints.MapRabbitMQHealthCheck();
            });
        }
    }
}
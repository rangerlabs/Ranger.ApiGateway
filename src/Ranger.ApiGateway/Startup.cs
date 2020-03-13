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
using Ranger.ApiGateway.Authorization;
using Ranger.ApiGateway.Data;
using Ranger.Common;
using Ranger.InternalHttpClient;
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

            services.AddAuthorization(options =>
            {
                options.AddPolicy("apiGateway", policyBuilder =>
                    {
                        policyBuilder.RequireScope("apiGateway");
                    });

                options.AddPolicy("BelongsToProject", policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser().AddRequirements(new BelongsToProjectRequirement());
                });
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthorizationHandler, BelongsToProjectHandler>();
            services.AddSingleton<ITenantsClient, TenantsClient>(provider =>
            {
                return new TenantsClient("http://tenants:8082", loggerFactory.CreateLogger<TenantsClient>());
            });
            services.AddSingleton<IProjectsClient, ProjectsClient>(provider =>
            {
                return new ProjectsClient("http://projects:8086", loggerFactory.CreateLogger<ProjectsClient>());
            });
            services.AddSingleton<IIdentityClient, IdentityClient>(provider =>
            {
                return new IdentityClient("http://identity:5000", loggerFactory.CreateLogger<IdentityClient>());
            });
            services.AddSingleton<IGeofencesClient, GeofencesClient>(provider =>
            {
                return new GeofencesClient("http://geofences:8085", loggerFactory.CreateLogger<GeofencesClient>());
            });
            services.AddSingleton<IOperationsClient, OperationsClient>(provider =>
            {
                return new OperationsClient("http://operations:8083", loggerFactory.CreateLogger<OperationsClient>());
            });
            services.AddSingleton<IIntegrationsClient, IntegrationsClient>(provider =>
            {
                return new IntegrationsClient("http://integrations:8087", loggerFactory.CreateLogger<IntegrationsClient>());
            });

            services.AddCors();

            services.AddApiVersioning(o => o.ApiVersionReader = new HeaderApiVersionReader("api-version"));

            services.AddEntityFrameworkNpgsql().AddDbContext<ApiGatewayDbContext>(options =>
            {
                options.UseNpgsql(configuration["cloudSql:ConnectionString"]);
            },
                ServiceLifetime.Transient
            );
            services.AddTransient<IApiGatewayDbContextInitializer, ApiGatewayDbContextInitializer>();

            services.AddDataProtection()
                .ProtectKeysWithCertificate(new X509Certificate2(configuration["DataProtectionCertPath:Path"]))
                .PersistKeysToDbContext<ApiGatewayDbContext>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "apiGateway";

                    options.RequireHttpsMetadata = false;
                });

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
            app.UseRouting();
            app.UseCors(builder =>
            {
                builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .WithExposedHeaders("X-Operation");
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
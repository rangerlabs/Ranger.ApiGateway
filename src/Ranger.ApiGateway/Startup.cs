using System;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Ranger.ApiGateway.Data;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    public class Startup
    {
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration configuration;
        private ILoggerFactory loggerFactory;
        private IBusSubscriber busSubscriber;

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
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("apiGateway", policyBuilder =>
                    {
                        policyBuilder.RequireScope("apiGateway");
                    });
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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

                    //TODO: Change these to true
                    options.EnableCaching = false;
                    options.RequireHttpsMetadata = false;
                });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.AddRabbitMq(loggerFactory);
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;

            app.UseRewriter(
                new RewriteOptions().AddRewrite(@"^api(.*)", "$1", true)
            );

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
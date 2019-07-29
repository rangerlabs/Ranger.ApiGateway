using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Ranger.ApiGateway;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway {
    public class Startup {
        private readonly IConfiguration configuration;
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<Startup> logger;
        private IContainer container;

        public Startup (IConfiguration configuration, ILoggerFactory loggerFactory, ILogger<Startup> logger) {
            this.configuration = configuration;
            this.loggerFactory = loggerFactory;
            this.logger = logger;
        }

        public IServiceProvider ConfigureServices (IServiceCollection services) {

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor> ();
            services.AddSingleton<ITenantsClient, TenantsClient> (provider => {
                return new TenantsClient ("http://tenants:8082", new LoggerFactory ().CreateLogger<TenantsClient> ());
            });
            services.AddSingleton<IIdentityClient, IdentityClient> (provider => {
                return new IdentityClient ("http://identity:5000", new LoggerFactory ().CreateLogger<IdentityClient> ());
            });
            services.AddCors ();
            services.AddMvc (options => {
                    var policy = ScopePolicy.Create ("apiGateway");
                    options.Filters.Add (new AuthorizeFilter (policy));
                })
                .SetCompatibilityVersion (CompatibilityVersion.Version_2_2)
                .AddJsonOptions (options => {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver ();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
            services.AddAuthentication ("Bearer")
                .AddIdentityServerAuthentication (options => {
                    options.Authority = "http://identity:5000/auth";
                    options.ApiName = "apiGateway";

                    //TODO: Change these to true
                    options.EnableCaching = false;
                    options.RequireHttpsMetadata = false;
                });

            var builder = new ContainerBuilder ();
            builder.Populate (services);
            builder.AddRabbitMq (loggerFactory);
            container = builder.Build ();
            return new AutofacServiceProvider (container);
        }

        public void Configure (IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            app.UseCors (builder => {
                builder.AllowAnyHeader ()
                    .AllowAnyMethod ()
                    .AllowAnyOrigin ()
                    .WithExposedHeaders ("X-Operation");
            });

            if (env.IsProduction ()) {
                app.UsePathBase ("/api");
            }
            app.UseAuthentication ();
            app.UseMvcWithDefaultRoute ();
        }
    }
}
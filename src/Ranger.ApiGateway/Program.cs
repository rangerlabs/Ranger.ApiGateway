using System;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ranger.ApiGateway.Data;
using Ranger.Monitoring.Logging;

namespace Ranger.ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build(); ;

            var host = BuildHost(config["serverBindingUrl"], args);

            using (var scope = host.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<IApiGatewayDbContextInitializer>();
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                dbInitializer.Migrate();
            }

            host.Run();
        }

        public static IWebHost BuildHost(string serverBindingUrl, string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseUrls(serverBindingUrl)
            .UseStartup<Startup>()
            .UseLogging()
            .ConfigureServices(services => services.AddAutofac())
            .Build();
    }
}
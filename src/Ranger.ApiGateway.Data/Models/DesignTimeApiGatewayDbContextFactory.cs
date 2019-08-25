using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Ranger.ApiGateway.Data {
    public class DesignTimeApiGatewayDbContextFactory : IDesignTimeDbContextFactory<ApiGatewayDbContext> {
        public ApiGatewayDbContext CreateDbContext (string[] args) {
            var config = new ConfigurationBuilder ()
                .SetBasePath (System.IO.Directory.GetCurrentDirectory ())
                .AddJsonFile ("appsettings.json")
                .Build ();

            var options = new DbContextOptionsBuilder<ApiGatewayDbContext> ();
            options.UseNpgsql (config["cloudSql:ConnectionString"]);

            return new ApiGatewayDbContext (options.Options);
        }
    }
}
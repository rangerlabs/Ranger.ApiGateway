using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway.Data
{
    public class ApiGatewayDbContext : DbContext, IDataProtectionKeyContext, IOutboxStore
    {

        private readonly IDataProtectionProvider dataProtectionProvider;
        public ApiGatewayDbContext(DbContextOptions<ApiGatewayDbContext> options, IDataProtectionProvider dataProtectionProvider = null) : base(options)
        {
            this.dataProtectionProvider = dataProtectionProvider;
        }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<RangerRabbitMessage> RangerRabbitMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            EncryptingDbHelper encryptionHelper = null;
            if (dataProtectionProvider != null)
            {
                encryptionHelper = new EncryptingDbHelper(this.dataProtectionProvider);
            }

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Remove 'AspNet' prefix and convert table name from PascalCase to snake_case. E.g. AspNetRoleClaims -> role_claims
                entity.SetTableName(entity.GetTableName().Replace("AspNet", "").ToSnakeCase());

                // Convert column names from PascalCase to snake_case.
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.Name.ToSnakeCase());
                }

                // Convert primary key names from PascalCase to snake_case. E.g. PK_users -> pk_users
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName().ToSnakeCase());
                }

                // Convert foreign key names from PascalCase to snake_case.
                foreach (var key in entity.GetForeignKeys())
                {
                    key.SetConstraintName(key.GetConstraintName().ToSnakeCase());
                }

                // Convert index names from PascalCase to snake_case.
                foreach (var index in entity.GetIndexes())
                {
                    index.SetName(index.GetName().ToSnakeCase());
                }

                encryptionHelper?.SetEncrytedPropertyAccessMode(entity);
            }
        }
    }
}
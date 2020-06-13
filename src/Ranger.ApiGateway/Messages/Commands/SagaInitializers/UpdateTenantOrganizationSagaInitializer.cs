using System;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway.Messages.Commands.Tenants
{
    [MessageNamespace("operations")]
    public class UpdateTenantOrganizationSagaInitializer : SagaInitializer, ICommand
    {
        public UpdateTenantOrganizationSagaInitializer(string commandingUserEmail, string tenantId, int version, string organizationName = "", string domain = "")
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new ArgumentException($"'{nameof(commandingUserEmail)}' cannot be null or whitespace", nameof(commandingUserEmail));
            }

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException($"'{nameof(tenantId)}' cannot be null or whitespace", nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(organizationName) && string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException($"'{nameof(organizationName)}' or '{nameof(domain)}' must not null or whitespace", nameof(organizationName));
            }

            if (version <= 1)
            {
                throw new ArgumentException($"'{nameof(version)}' must be greater than 1", nameof(version));
            }

            CommandingUserEmail = commandingUserEmail;
            TenantId = tenantId;
            OrganizationName = organizationName;
            Domain = domain;
            Version = version;
        }

        public string Domain { get; }
        public string OrganizationName { get; }
        public string CommandingUserEmail { get; }
        public int Version { get; }
    }
}
using System;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway.Messages.Commands.Tenants
{
    [MessageNamespace("operations")]
    public class UpdateTenantOrganizationNameSagaInitializer : SagaInitializer, ICommand
    {
        public UpdateTenantOrganizationNameSagaInitializer(string commandingUserEmail, string tenantid, string organizationName)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new ArgumentException($"'{nameof(commandingUserEmail)}' cannot be null or whitespace", nameof(commandingUserEmail));
            }

            if (string.IsNullOrWhiteSpace(tenantid))
            {
                throw new ArgumentException($"'{nameof(tenantid)}' cannot be null or whitespace", nameof(tenantid));
            }

            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new ArgumentException($"'{nameof(organizationName)}' cannot be null or whitespace", nameof(organizationName));
            }
            CommandingUserEmail = commandingUserEmail;
            TenantId = tenantid;
            OrganizationName = organizationName;
        }

        public string OrganizationName { get; }
        public string CommandingUserEmail { get; }
    }
}
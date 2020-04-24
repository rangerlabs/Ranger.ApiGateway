using System;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespaceAttribute("operations")]
    public class DeleteTenantSagaInitializer : SagaInitializer, ICommand
    {
        public DeleteTenantSagaInitializer(string commandingUserEmail, string tenantid)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(tenantid))
            {
                throw new System.ArgumentException($"{nameof(tenantid)} was null or whitespace");
            }

            this.CommandingUserEmail = commandingUserEmail;
            TenantId = tenantid;
        }

        public string CommandingUserEmail { get; }
    }
}
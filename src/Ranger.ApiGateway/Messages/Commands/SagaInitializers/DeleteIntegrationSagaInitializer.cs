using System;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespaceAttribute("operations")]
    public class DeleteIntegrationSagaInitializer : SagaInitializer, ICommand
    {
        public DeleteIntegrationSagaInitializer(string commandingUserEmail, string tenantid, string name, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(tenantid))
            {
                throw new System.ArgumentException($"{nameof(tenantid)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new System.ArgumentException($"{nameof(name)} was null or whitespace");
            }


            this.CommandingUserEmail = commandingUserEmail;

            TenantId = tenantid;
            this.Name = name;
            this.ProjectId = projectId;
        }
        public string CommandingUserEmail { get; }
        public string Name { get; }
        public Guid ProjectId { get; }
    }
}
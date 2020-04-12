using System;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway.Messages.Commands
{
    [MessageNamespaceAttribute("operations")]
    public class CreateIntegrationSagaInitializer : SagaInitializer, ICommand
    {
        public CreateIntegrationSagaInitializer(
            string commandingUserEmail,
            string tenantid,
            string name,
            Guid projectId,
            string messageJsonContent,
            IntegrationsEnum integrationType)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace.");
            }

            if (string.IsNullOrEmpty(tenantid))
            {
                throw new ArgumentException($"{nameof(tenantid)} was null or whitespace.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"{nameof(name)} was null or whitespace.");
            }

            if (string.IsNullOrEmpty(messageJsonContent))
            {
                throw new ArgumentException($"{nameof(messageJsonContent)} was null or whitespace.");
            }

            CommandingUserEmail = commandingUserEmail;
            TenantId = tenantid;
            Name = name;
            ProjectId = projectId;
            MessageJsonContent = messageJsonContent;
            IntegrationType = integrationType;
        }

        public string CommandingUserEmail { get; }
        public string Name { get; }
        public Guid ProjectId { get; }
        public string MessageJsonContent { get; }
        public IntegrationsEnum IntegrationType { get; }
    }
}
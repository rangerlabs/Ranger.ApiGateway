using System;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway.Messages.Commands
{
    [MessageNamespaceAttribute("operations")]
    public class UpdateIntegrationSagaInitializer : SagaInitializer, ICommand
    {
        public UpdateIntegrationSagaInitializer(
            string commandingUserEmail,
            string tenantid,
            string name,
            Guid projectId,
            string messageJsonContent,
            IntegrationsEnum integrationType,
            int version)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmail))
            {
                throw new ArgumentException($"{nameof(commandingUserEmail)} was null or whitespace");
            }

            if (string.IsNullOrEmpty(tenantid))
            {
                throw new ArgumentException($"{nameof(tenantid)} was null or whitespace");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"{nameof(name)} was null or whitespace");
            }

            if (string.IsNullOrEmpty(messageJsonContent))
            {
                throw new ArgumentException($"{nameof(messageJsonContent)} was null or whitespace");
            }
            if (version <= 0)
            {
                throw new ArgumentOutOfRangeException("Version must be a positive integer");
            }

            CommandingUserEmail = commandingUserEmail;
            TenantId = tenantid;
            Name = name;
            ProjectId = projectId;
            MessageJsonContent = messageJsonContent;
            IntegrationType = integrationType;
            Version = version;
        }

        public string CommandingUserEmail { get; }
        public string Name { get; }
        public Guid ProjectId { get; }
        public string MessageJsonContent { get; }
        public IntegrationsEnum IntegrationType { get; }
        public int Version { get; }
    }
}
using System;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespaceAttribute("operations")]
    public class DeleteGeofenceSagaInitializer : SagaInitializer, ICommand
    {
        public DeleteGeofenceSagaInitializer(bool frontendRequest, string commandingUserEmailOrTokenPrefix, string tenantid, string externalId, Guid projectId)
        {
            if (string.IsNullOrWhiteSpace(commandingUserEmailOrTokenPrefix))
            {
                throw new System.ArgumentException($"{nameof(commandingUserEmailOrTokenPrefix)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(tenantid))
            {
                throw new System.ArgumentException($"{nameof(tenantid)} was null or whitespace");
            }
            if (string.IsNullOrWhiteSpace(externalId))
            {
                throw new System.ArgumentException($"{nameof(externalId)} was null or whitespace");
            }


            this.FrontendRequest = frontendRequest;
            this.CommandingUserEmailOrTokenPrefix = commandingUserEmailOrTokenPrefix;

            TenantId = tenantid;
            this.ExternalId = externalId;
            this.ProjectId = projectId;
        }
        public bool FrontendRequest { get; }
        public string CommandingUserEmailOrTokenPrefix { get; }
        public string ExternalId { get; }
        public Guid ProjectId { get; }
    }
}
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespace("notifications")]
    public class SendPusherDomainUserPredefinedNotification : ICommand
    {
        public string BackendEventKey { get; }
        public string TenantId { get; }
        public string UserEmail { get; }

        public SendPusherDomainUserPredefinedNotification(string backendEventKey, string tenantId, string userEmail)
        {
            this.BackendEventKey = backendEventKey;
            this.TenantId = tenantId;
            this.UserEmail = userEmail;
        }
    }
}
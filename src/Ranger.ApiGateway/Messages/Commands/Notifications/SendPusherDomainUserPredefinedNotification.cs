using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespace("notifications")]
    public class SendPusherDomainUserPredefinedNotification : ICommand
    {
        public string BackendEventKey { get; }
        public string Domain { get; }
        public string UserEmail { get; }

        public SendPusherDomainUserPredefinedNotification(string backendEventKey, string domain, string userEmail)
        {
            this.BackendEventKey = backendEventKey;
            this.Domain = domain;
            this.UserEmail = userEmail;
        }
    }
}
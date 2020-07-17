using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [MessageNamespace("notifications")]
    public class SendContactFormEmail : ICommand
    {
        public SendContactFormEmail(string organization, string email, string message)
        {
            this.Organization = organization;
            this.Email = email;
            this.Message = message;

        }
        public string Organization { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }
}
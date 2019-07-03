using System.Threading.Tasks;

namespace Ranger.ApiGateway {
    public interface IEmailSender {
        Task SendEmailAsync (string toEmail, string subject, string htmlMessage, string textMessage = null);
    }
}
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway {
    [MessageNamespace ("identity")]
    public class ApplicationUserModel {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }
        public string TenantDomain { get; set; }
        public Role Role { get; set; }
        public string PhoneNumber { get; set; }
    }
}
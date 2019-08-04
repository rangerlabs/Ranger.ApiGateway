using System.ComponentModel.DataAnnotations;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway {
    public class ApplicationUserModel {
        [Required]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Role { get; set; }
        public string TenantDomain { get; set; }
        public string PhoneNumber { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway {
    public class ApplicationUserApiResponseModel {

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Role { get; set; }
    }
}
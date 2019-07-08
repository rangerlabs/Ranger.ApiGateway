namespace Ranger.ApiGateway {
    public class ApplicationUserResponseModel {

        public string Email { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string TenantDomain { get; set; }
        public string Role { get; set; }
        public string PhoneNumber { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class UserForm
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string ReCaptchaToken { get; set; }
    }
}
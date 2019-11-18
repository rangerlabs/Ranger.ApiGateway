using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class UserConfirmModel
    {
        [Required]
        public string RegistrationKey { get; set; }
        public string Domain { get; set; }
    }
}
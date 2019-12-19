using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class PasswordResetModel
    {
        [Required]
        public string Password { get; set; }
    }
}
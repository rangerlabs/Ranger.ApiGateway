using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class UserConfirmEmailChangeModel
    {
        [Required]
        public string Domain { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class AccountDeleteModel
    {
        [Required]
        public string Password { get; set; }
    }
}
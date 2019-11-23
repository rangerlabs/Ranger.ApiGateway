using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiUtilities
{
    public class PasswordResetModel
    {
        [Required]
        public string Password { get; set; }
    }
}
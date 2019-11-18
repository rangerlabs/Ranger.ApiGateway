using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class UserConfirmPasswordResetModel
    {
        [Required]
        public string Domain { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        [StringLength(124, MinimumLength = 8)]
        [RegularExpression(@"[!@#$%^&*)(+=._-]{1}[a-z]{1}[A-Z]{1}[0-9]{1}")]
        public string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}
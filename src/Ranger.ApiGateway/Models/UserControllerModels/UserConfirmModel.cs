using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class UserConfirmModel
    {
        public string Domain { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        [StringLength(64, MinimumLength = 8)]
        [RegularExpression(@"^(?!.*\s)(?=.*[`~!@#\$%\^&\*\(\)_\\\+-=\{\}\[\]\|;:'"",<\.>/\?])(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,64}$")]
        public string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway.Models.AccountControllerModels
{
    public class ChangePasswordModel
    {
        [Required]
        [StringLength(64, ErrorMessage = "Passwords should be between 8 and 64 characters", MinimumLength = 8)]
        [RegularExpression(@"^(?!.*\s)(?=.*[`~!@#\$%\^&\*\(\)_\\\+-=\{\}\[\]\|;:'"",<\.>/\?])(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,64}$")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

    }
}
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway.Models.AccountControllerModels
{
    public class ChangePasswordModel
    {
        [Required]
        [StringLength(64, ErrorMessage = "Passwords should be between 6 and 64 characters", MinimumLength = 6)]
        [RegularExpression(@"[!@#$%^&*)(+=._-]{1}[a-z]{1}[A-Z]{1}[0-9]{1}")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

    }
}
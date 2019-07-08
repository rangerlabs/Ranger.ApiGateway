using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway {
    public class UserForm {

        [Required]
        [EmailAddress (ErrorMessage = "Invalid email address")]
        [Display (Name = "email")]
        public string Email { get; set; }

        [Required]
        [RegularExpression (@"^[a-zA-Z,.'-]{1}[a-zA-Z ,.'-]{1,46}[a-zA-Z,.'-]{1}$")]
        [StringLength (48, MinimumLength = 1)]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression (@"^[a-zA-Z,.'-]{1}[a-zA-Z ,.'-]{1,46}[a-zA-Z,.'-]{1}$")]
        [StringLength (48, MinimumLength = 1)]
        public string LastName { get; set; }

        [Required]
        [StringLength (64, ErrorMessage = "Passwords should be between 6 and 64 characters", MinimumLength = 6)]
        [DataType (DataType.Password)]
        public string Password { get; set; }

        [DataType (DataType.Password)]
        [Compare ("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

    }
}
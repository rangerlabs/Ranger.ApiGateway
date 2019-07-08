using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway {
    public class TenantModel {
        public DomainForm DomainForm { get; set; }
        public UserForm UserForm { get; set; }
    }

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

    public class DomainForm {
        [Required]
        [RegularExpression (@"^[a-zA-Z0-9]{1}[a-zA-Z0-9-]{1,26}[a-zA-Z0-9]{1}$")]
        [StringLength (28, MinimumLength = 3)]
        public string Domain { get; set; }

        [Required]
        [RegularExpression (@"^[a-zA-Z0-9]{1}[a-zA-Z0-9_- ]{1,26}[a-zA-Z0-9]{1}$")]
        [StringLength (48)]

        public string OrganizationName { get; set; }

    }
}
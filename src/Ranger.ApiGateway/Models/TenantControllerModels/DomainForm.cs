using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway {
    public class DomainForm {
        [Required]
        [RegularExpression (@"^[a-zA-Z0-9]{1}[a-zA-Z0-9-]{1,26}[a-zA-Z0-9]{1}$")]
        [StringLength (28, MinimumLength = 3)]
        public string Domain { get; set; }

        [Required]
        [RegularExpression (@"^[a-zA-Z0-9]{1}[a-zA-Z0-9- ]{1,26}[a-zA-Z0-9]{1}$")]
        [StringLength (48)]

        public string OrganizationName { get; set; }

    }
}
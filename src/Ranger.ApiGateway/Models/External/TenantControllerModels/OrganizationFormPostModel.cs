using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class OrganizationFormPostModel
    {
        [Required]
        [RegularExpression(@"^([\s\,\.\-\'a-zA-Z]){1,48}$")]
        [StringLength(28, MinimumLength = 3)]
        public string Domain { get; set; }

        [Required]
        [RegularExpression(@"^([\s\,\.\-\'a-zA-Z]){1,48}$")]
        [StringLength(48)]
        public string OrganizationName { get; set; }
    }
}
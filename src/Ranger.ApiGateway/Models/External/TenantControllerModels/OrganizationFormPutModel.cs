using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class OrganizationFormPutModel
    {
        [RegularExpression(@"^([\s\,\.\-\'a-zA-Z]){1,48}$")]
        [StringLength(28, MinimumLength = 3)]
        public string Domain { get; set; }

        [RegularExpression(@"^([\s\,\.\-\'a-zA-Z]){1,48}$")]
        [StringLength(48)]
        public string OrganizationName { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Only positive values allowed")]
        public int Version { get; set; }
    }
}
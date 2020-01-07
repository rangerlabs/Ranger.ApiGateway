using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class PostApplicationUserModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^([\s\,\.\-\'a-zA-Z]){1,48}$")]
        [StringLength(48, MinimumLength = 1)]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^([\s\,\.\-\'a-zA-Z]){1,48}$")]
        [StringLength(48, MinimumLength = 1)]
        public string LastName { get; set; }

        [Required]
        [EnumDataType(typeof(RolesEnum))]
        public string Role { get; set; }

        public IList<string> AuthorizedProjects { get; set; } = new List<string>();
    }
}
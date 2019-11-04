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
        [RegularExpression(@"^[a-zA-Z,.'-]{1}[a-zA-Z ,.'-]{1,46}[a-zA-Z,.'-]{1}$")]
        [StringLength(48, MinimumLength = 1)]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z,.'-]{1}[a-zA-Z ,.'-]{1,46}[a-zA-Z,.'-]{1}$")]
        [StringLength(48, MinimumLength = 1)]
        public string LastName { get; set; }

        [Required]
        [EnumDataType(typeof(RolesEnum))]
        public string Role { get; set; }

        public IList<string> PermittedProjects { get; set; } = new List<string>();
    }
}
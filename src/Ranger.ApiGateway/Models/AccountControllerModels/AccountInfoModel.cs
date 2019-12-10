using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway.Models.AccountControllerModels
{
    public class AccountInfoModel
    {
        [Required]
        [RegularExpression(@"^([\s\,\.\-\'a-zA-Z]){1,48}$")]
        [StringLength(48, MinimumLength = 1)]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^([\s\,\.\-\'a-zA-Z]){1,48}$")]
        [StringLength(48, MinimumLength = 1)]
        public string LastName { get; set; }
    }
}
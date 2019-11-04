using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway.Models.AccountControllerModels
{
    public class AccountInfoModel
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z,.'-]{1}[a-zA-Z ,.'-]{1,46}[a-zA-Z,.'-]{1}$")]
        [StringLength(48, MinimumLength = 1)]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z,.'-]{1}[a-zA-Z ,.'-]{1,46}[a-zA-Z,.'-]{1}$")]
        [StringLength(48, MinimumLength = 1)]
        public string LastName { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway.Models.AccountControllerModels
{
    public class EmailChangeModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [EmailAddress]
        [Compare("Email")]
        public string ConfirmEmail { get; set; }
    }
}
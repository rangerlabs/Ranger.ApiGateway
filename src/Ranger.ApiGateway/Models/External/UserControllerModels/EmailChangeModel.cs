using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class EmailChangeModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
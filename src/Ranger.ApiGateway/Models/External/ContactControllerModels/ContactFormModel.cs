using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class ContactFormModel
    {
        [Required]
        [StringLength(512)]
        public string Organization { get; set; }
        [Required]
        [StringLength(512)]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        [StringLength(512)]
        public string Email { get; set; }
        [Required]
        [StringLength(1000)]
        public string Message { get; set; }
    }
}
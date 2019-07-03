using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway {
    public class NewUser {
        [Required]
        [EmailAddress]
        public string username { get; set; }

        [Required]
        [MinLength (8)]
        public string password { get; set; }

        [Required]
        public string tenant { get; set; }
    }
}
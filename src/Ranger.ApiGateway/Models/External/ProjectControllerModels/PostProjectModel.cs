using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class PostProjectModel
    {

        [Required]
        [StringLength(140)]
        public string Name { get; set; }
        [Required]
        public bool Enabled { get; set; }
        public string Description { get; set; }
    }
}
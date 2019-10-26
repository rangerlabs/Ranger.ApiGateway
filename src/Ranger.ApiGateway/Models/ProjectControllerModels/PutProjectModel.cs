using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class PutProjectModel
    {

        [Required]
        [StringLength(140)]
        public string Name { get; set; }
        [Required]
        public bool Enabled { get; set; }
        [Required]
        public int Version { get; set; }
        public string Description { get; set; }
    }
}
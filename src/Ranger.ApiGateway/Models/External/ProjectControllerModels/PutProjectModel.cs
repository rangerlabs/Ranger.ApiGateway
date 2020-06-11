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
        [Range(1, int.MaxValue, ErrorMessage = "Only positive values allowed")]
        public int Version { get; set; }
        public string Description { get; set; }
    }
}
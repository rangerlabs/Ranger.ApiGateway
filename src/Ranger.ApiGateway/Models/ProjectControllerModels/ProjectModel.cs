using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class ProjectModel
    {

        [Required]
        [StringLength(140)]
        public string Name { get; set; }
        [Required]
        public int Version { get; set; }
        public string Description { get; set; }
        public string ApiKey { get; set; }
    }
}
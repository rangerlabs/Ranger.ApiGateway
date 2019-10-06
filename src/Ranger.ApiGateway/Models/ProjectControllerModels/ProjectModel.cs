using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class ProjectModel
    {

        [Required]
        [StringLength(140)]
        public string Name { get; set; }
        public string Description { get; set; }

    }
}
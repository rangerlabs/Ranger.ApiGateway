using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class ProjectModel
    {

        [Required]
        public string Name { get; set; }

        [StringLength(140)]
        public string Description { get; set; }

    }
}
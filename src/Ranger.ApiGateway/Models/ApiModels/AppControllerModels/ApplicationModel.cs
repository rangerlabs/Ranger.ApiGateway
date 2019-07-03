using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway {
    public class ApplicationModel {

        [Required]
        public string Name { get; set; }

        [Required]
        [StringLength (140)]
        public string Description { get; set; }

    }
}
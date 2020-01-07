using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class WebhookIntegrationModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public string URL { get; set; }
        [Required]
        public string AuthKey { get; set; }
    }
}
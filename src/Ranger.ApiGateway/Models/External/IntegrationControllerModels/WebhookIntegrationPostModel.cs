using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class WebhookIntegrationPostModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string URL { get; set; }
        public string Description { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Headers { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
    }
}
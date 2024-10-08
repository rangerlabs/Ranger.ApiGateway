using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class WebhookIntegrationPostModel
    {
        public bool Enabled { get; set; } = true;
        public EnvironmentEnum Environment { get; set; }
        public bool IsDefault { get; set; } = false;
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Headers { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
    }
}
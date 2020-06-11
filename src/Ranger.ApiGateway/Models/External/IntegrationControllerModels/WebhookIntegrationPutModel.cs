using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class WebhookIntegrationPutModel
    {
        public Guid IntegrationId { get; set; }
        [Required]
        public bool Enabled { get; set; }
        [Required]
        [StringLength(140)]
        public string Name { get; set; }
        [Required]
        public EnvironmentEnum Environment { get; set; }
        [Required]
        [Url]
        public string URL { get; set; }
        public string Description { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Headers { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Only positive values allowed")]
        public int Version { get; set; }
    }
}
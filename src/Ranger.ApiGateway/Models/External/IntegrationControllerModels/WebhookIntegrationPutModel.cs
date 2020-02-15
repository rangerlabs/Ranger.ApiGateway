using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ranger.Common.SharedKernel;

namespace Ranger.ApiGateway
{
    public class WebhookIntegrationPutModel
    {
        public Guid Id { get; set; }
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
        public int Version { get; set; }
    }
}
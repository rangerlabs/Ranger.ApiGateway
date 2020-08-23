using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class WebhookIntegrationPutModel
    {
        public Guid Id { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; } = false;
        public EnvironmentEnum Environment { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Headers { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
        public int Version { get; set; }
    }
}
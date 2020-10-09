using System;
using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class PusherIntegrationPutModel
    {
        public Guid Id { get; set; }
        public bool Enabled { get; set; }
        public EnvironmentEnum Environment { get; set; }
        public bool IsDefault { get; set; } = false;
        public string Name { get; set; }
        public string Description { get; set; }
        public string AppId { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }
        public string Cluster { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
        public int Version { get; set; }
    }
}
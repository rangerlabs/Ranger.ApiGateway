using System;
using System.Collections.Generic;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class GeofenceRequestModel
    {
        public string ExternalId { get; set; }
        public IEnumerable<LngLat> Coordinates { get; set; }
        public bool OnEnter { get; set; } = true;
        public bool OnDwell { get; set; } = false;
        public bool OnExit { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public string Description { get; set; }
        public IEnumerable<Guid> IntegrationIds { get; set; }
        public int Radius { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
        public GeofenceShapeEnum Shape { get; set; }
        public Schedule Schedule { get; set; }
    }
}
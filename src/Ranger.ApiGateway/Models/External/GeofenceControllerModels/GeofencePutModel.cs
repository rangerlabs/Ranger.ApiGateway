using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class GeofencePutModel
    {
        [Required]
        public IEnumerable<LngLat> Coordinates { get; set; }
        public IEnumerable<string> Labels { get; set; }
        public bool OnEnter { get; set; } = true;
        public bool OnExit { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public string Description { get; set; }
        public IEnumerable<string> IntegrationIds { get; set; }
        public int Radius { get; set; }
        public IDictionary<string, object> Metadata { get; set; }
        public GeofenceShapeEnum Shape { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime LaunchDate { get; set; }
        public Schedule Schedule { get; set; }
    }
}
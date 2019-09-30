using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class GeofenceModel
    {
        [Required]
        public string Name { get; set; }
        public IEnumerable<string> Labels { get; set; }
        public bool OnEnter { get; set; } = true;
        public bool OnExit { get; set; } = true;
        public bool Enabled { get; set; } = true;
        [Required]
        public string Description { get; set; }
        public IEnumerable<string> IntegrationNames { get; set; }
        [Required]
        public IEnumerable<LatLng> Coordinates { get; set; }
        [Required]
        public int Radius { get; set; }
        public IDictionary<string, object> Metadata { get; set; }
        public GeofenceShapeEnum Shape { get; }
    }
}
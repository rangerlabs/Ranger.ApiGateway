using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class GeofencePutModel
    {
        [Required]
        [StringLength(140)]
        public string ExternalId { get; set; }
        [Required]
        public IEnumerable<LngLat> Coordinates { get; set; }
        public IEnumerable<string> Labels { get; set; }
        public bool OnEnter { get; set; } = true;
        public bool OnDwell { get; set; } = true;
        public bool OnExit { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public string Description { get; set; }
        public IEnumerable<Guid> IntegrationIds { get; set; }
        public int Radius { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
        [Required]
        public GeofenceShapeEnum Shape { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? LaunchDate { get; set; }
        public Schedule Schedule { get; set; }
    }
}
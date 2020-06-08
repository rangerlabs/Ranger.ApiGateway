using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class BreadcrumbModel
    {
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public LngLat Position { get; set; }
        [Required]
        public double Accuracy { get; set; }
        [Required]
        public DateTime RecordedAt { get; set; }
        public string ExternalUserId { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; } = new List<KeyValuePair<string, string>>();
    }
}
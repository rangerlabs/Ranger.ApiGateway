using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ranger.Common;

namespace Ranger.ApiGateway.Models.External.BreadcrumbsControllerModels
{
    public class Breadcrumb
    {
        [Required]
        public string DeviceId { get; set; }
        [StringLength(48)]
        public string ExternalUserId { get; set; }
        [Required]
        public LngLat Position { get; set; }
        public double Accuracy { get; set; }
        public DateTime RecordedAt { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway.Models.External.BreadcrumbsControllerModels
{
    public class Breadcrumb
    {
        [Required]
        public string DeviceId { get; set; }
        [StringLength(48)]
        public string ExternalUserId { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        public double Accuracy { get; set; }
        public DateTime RecordedAt { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
    }
}
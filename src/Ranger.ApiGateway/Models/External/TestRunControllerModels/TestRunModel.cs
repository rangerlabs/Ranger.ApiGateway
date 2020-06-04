using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ranger.Common;

namespace Ranger.ApiGateway
{
    public class TestRunModel
    {
        [Required]
        public IEnumerable<LngLat> Positions { get; set; }
    }
}
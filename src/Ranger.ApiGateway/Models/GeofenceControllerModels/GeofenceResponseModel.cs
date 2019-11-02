using System.Collections.Generic;

namespace Ranger.ApiGateway
{
    public class GeofenceResponseModel
    {
        public string ProjectName { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Labels { get; set; }
        public bool OnEnter { get; set; } = true;
        public bool OnExit { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public string Description { get; set; }
        public IEnumerable<string> IntegrationIds { get; set; }
        public IEnumerable<LatLng> Coordinates { get; set; }
        public int Radius { get; set; }
        public IDictionary<string, object> Metadata { get; set; }
        public string Shape { get; set; }
    }
}

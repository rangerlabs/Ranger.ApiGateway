using System;
using System.Collections;
using System.Collections.Generic;

namespace Ranger.ApiGateway
{
    public class CircularGeoFenceApiResponseModel : IGeoFenceApiResponseModel
    {
        public CircularGeoFenceApiResponseModel()
        {
            this.Shape = Enum.GetName(typeof(GeoFenceShapeEnum), GeoFenceShapeEnum.Circular);
        }

        public string Id { get; set; }
        public string AppId { get; set; }
        public IEnumerable<string> IntegrationIds { get; set; } = new List<string>();
        public string Name { get; set; }
        public string Description { get; set; }
        public string Shape { get; private set; }
        public LatLng Center { get; set; }
        public int Radius { get; set; }
        public bool OnEnter { get; set; }
        public bool OnExit { get; set; }

    }
}
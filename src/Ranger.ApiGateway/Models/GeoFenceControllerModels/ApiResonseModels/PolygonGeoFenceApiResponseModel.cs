using System;
using System.Collections.Generic;

namespace Ranger.ApiGateway
{
    public class PolygonGeoFenceApiResponseModel : IGeoFenceApiResponseModel
    {
        public PolygonGeoFenceApiResponseModel()
        {
            this.Shape = Enum.GetName(typeof(GeoFenceShapeEnum), GeoFenceShapeEnum.Polygon);
        }

        public string Id { get; set; }
        public string AppId { get; set; }
        public IEnumerable<string> IntegrationIds { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Shape { get; private set; }
        public IEnumerable<LatLng> LatLngPath { get; set; }
        public bool OnEnter { get; set; }
        public bool OnExit { get; set; }

    }
}
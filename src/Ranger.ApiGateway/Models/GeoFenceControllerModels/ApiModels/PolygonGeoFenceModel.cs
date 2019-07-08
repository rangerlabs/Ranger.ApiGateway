using System.Collections.Generic;

namespace Ranger.ApiGateway {
    public class PolygonGeoFenceModel : IGeoFenceModel {
        public PolygonGeoFenceModel () {
            this.Shape = GeoFenceShapeEnum.Polygon;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public GeoFenceShapeEnum Shape { get; private set; }
        public IEnumerable<LatLng> LatLngPath { get; set; }
        public bool OnEnter { get; set; }
        public bool OnExit { get; set; }
    }
}
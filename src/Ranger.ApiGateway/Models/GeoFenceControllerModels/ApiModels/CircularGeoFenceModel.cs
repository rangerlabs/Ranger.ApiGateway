namespace Ranger.ApiGateway {
    public class CircularGeoFenceModel : IGeoFenceModel {
        public CircularGeoFenceModel () {
            this.Shape = GeoFenceShapeEnum.Circular;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public GeoFenceShapeEnum Shape { get; private set; }
        public LatLng Center { get; set; }
        public int Radius { get; set; }
        public bool OnEnter { get; set; }
        public bool OnExit { get; set; }

    }
}
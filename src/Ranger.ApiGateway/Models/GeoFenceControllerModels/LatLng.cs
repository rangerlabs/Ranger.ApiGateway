namespace Ranger.ApiGateway {
    public class LatLng {
        public LatLng (double Lat, double Lng) {
            this.Lat = Lat;
            this.Lng = Lng;
        }

        public double Lat { get; private set; }
        public double Lng { get; private set; }
    }
}
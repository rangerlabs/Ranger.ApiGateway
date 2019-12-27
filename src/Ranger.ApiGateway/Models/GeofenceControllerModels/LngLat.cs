namespace Ranger.ApiGateway
{
    public class LatLng
    {
        public LatLng(double lng, double lat)
        {
            this.Lng = Lng;
            this.Lat = Lat;
        }

        public double Lat { get; private set; }
        public double Lng { get; private set; }
    }
}
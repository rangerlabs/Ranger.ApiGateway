namespace Ranger.ApiGateway
{
    public class GeofenceRequestParams
    {
        public GeofenceRequestParams(string geofenceSortOrder, string orderByOption, int page, int pageCount)
        {
            this.GeofenceSortOrder = geofenceSortOrder;
            this.OrderByOption = orderByOption;
            this.Page = page;
            this.PageCount = pageCount;

        }
        public string GeofenceSortOrder { get; }
        public string OrderByOption { get; }
        public int Page { get; }
        public int PageCount { get; }
    }
}
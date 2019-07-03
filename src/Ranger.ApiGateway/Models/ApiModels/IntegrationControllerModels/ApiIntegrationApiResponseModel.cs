namespace Ranger.ApiGateway {
    public class ApiIntegrationApiResponseModel {
        public ApiIntegrationApiResponseModel () {
            this.Type = "API";
        }
        public string Type { get; set; }
        public string AppName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string HttpEndpoint { get; set; }
        public string AuthKey { get; set; }

    }
}
namespace Ranger.ApiGateway {
    public class ApiIntegrationModel {
        public string AppName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string HttpEndpoint { get; set; }
        public string AuthKey { get; set; }
    }
}
using System;

namespace Ranger.ApiGateway
{
    public class WebhookIntegrationResponseModel
    {
        public WebhookIntegrationResponseModel()
        {
            this.Type = Enum.GetName(typeof(IntegrationEnum), IntegrationEnum.WEBHOOK);
        }
        public string Type { get; set; }
        public string Name { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public string AuthKey { get; set; }
    }
}
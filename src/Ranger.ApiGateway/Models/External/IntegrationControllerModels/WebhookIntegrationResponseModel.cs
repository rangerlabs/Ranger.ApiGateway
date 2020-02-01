using System;
using System.Collections.Generic;
using Ranger.Common.SharedKernel;

namespace Ranger.ApiGateway
{
    public class WebhookIntegrationResponseModel
    {
        public WebhookIntegrationResponseModel()
        {
            this.Type = Enum.GetName(typeof(IntegrationsEnum), IntegrationsEnum.WEBHOOK);
        }
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Headers { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Metadata { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ranger.ApiUtilities;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "User")]
    [TenantDomainRequired]
    public class IntegrationController : ControllerBase
    {
        [HttpGet("{projectName}/integrations")]
        public async Task<IActionResult> All(string projectName)
        {
            IActionResult response = new StatusCodeResult(202);
            var appResponseCollection = new List<WebhookIntegrationResponseModel>();
            var appModel1 = new WebhookIntegrationResponseModel()
            {
                Id = "GUID1",
                ProjectName = "APP_ID_1",
                Name = "API_Integration",
                Description = "This is a test integration",
                URL = "http://someapi.com/",
                AuthKey = Guid.NewGuid().ToString()
            };
            var appModel2 = new WebhookIntegrationResponseModel()
            {
                Id = "GUID2",
                ProjectName = "APP_ID_2",
                Name = "API_Integration_2",
                Description = "This is another test integration",
                URL = "http://someapi.com/",
                AuthKey = Guid.NewGuid().ToString()
            };
            var appModel3 = new WebhookIntegrationResponseModel()
            {
                Id = "GUID3",
                ProjectName = "APP_ID_2",
                Name = "API_Integration_3",
                Description = "This is another test integration",
                URL = "http://someapi3.com/",
                AuthKey = Guid.NewGuid().ToString()
            };

            appResponseCollection.Add(appModel1);
            appResponseCollection.Add(appModel2);
            appResponseCollection.Add(appModel3);
            return Ok(appResponseCollection.Where(a => a.ProjectName == projectName));
        }
    }
}
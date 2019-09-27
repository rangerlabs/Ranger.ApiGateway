using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ranger.ApiUtilities;

namespace Ranger.ApiGateway
{
    [ApiController]
    [Authorize(Roles = "User")]
    [TenantDomainRequired]
    public class IntegrationController : ControllerBase
    {
        [HttpGet("/integration/all")]
        public async Task<IActionResult> All()
        {
            IActionResult response = new StatusCodeResult(202);
            var appResponseCollection = new List<ApiIntegrationApiResponseModel>();
            var appModel1 = new ApiIntegrationApiResponseModel()
            {
                Id = "INTEGATION_ID_1",
                AppId = "APP_ID_1",
                Name = "API Integration",
                Description = "This is a test integration",
                HttpEndpoint = "http://someapi.com/",
                AuthKey = Guid.NewGuid().ToString()
            };
            var appModel2 = new ApiIntegrationApiResponseModel()
            {
                Id = "INTEGATION_ID_2",
                AppId = "APP_ID_2",
                Name = "API Integration 2",
                Description = "This is another test integration",
                HttpEndpoint = "http://someapi.com/",
                AuthKey = Guid.NewGuid().ToString()
            };
            var appModel3 = new ApiIntegrationApiResponseModel()
            {
                Id = "INTEGATION_ID_3",
                AppId = "APP_ID_2",
                Name = "API Integration 3",
                Description = "This is another test integration",
                HttpEndpoint = "http://someapi3.com/",
                AuthKey = Guid.NewGuid().ToString()
            };

            appResponseCollection.Add(appModel1);
            appResponseCollection.Add(appModel2);
            appResponseCollection.Add(appModel3);
            return Ok(appResponseCollection);
        }
    }
}
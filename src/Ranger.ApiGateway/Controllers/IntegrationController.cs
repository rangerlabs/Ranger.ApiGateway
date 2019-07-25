using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ranger.ApiGateway {
    [ApiController]
    [Authorize (Roles = "User")]
    public class IntegrationController : ControllerBase {
        [HttpGet ("/integration/all")]
        public async Task<IActionResult> All (string email) {
            IActionResult response = new StatusCodeResult (202);
            var appResponseCollection = new List<ApiIntegrationApiResponseModel> ();
            var appModel1 = new ApiIntegrationApiResponseModel () {
                AppName = "TestApp",
                Name = "API Integration",
                Description = "This is a test integration",
                HttpEndpoint = "http://someapi.com/",
                AuthKey = Guid.NewGuid ().ToString ()
            };
            var appModel2 = new ApiIntegrationApiResponseModel () {
                AppName = "TestApp2",
                Name = "API Integration 2",
                Description = "This is another test integration",
                HttpEndpoint = "http://someapi.com/",
                AuthKey = Guid.NewGuid ().ToString ()
            };
            var appModel3 = new ApiIntegrationApiResponseModel () {
                AppName = "TestApp",
                Name = "API Integration 3",
                Description = "This is another test integration",
                HttpEndpoint = "http://someapi3.com/",
                AuthKey = Guid.NewGuid ().ToString ()
            };

            appResponseCollection.Add (appModel1);
            appResponseCollection.Add (appModel2);
            appResponseCollection.Add (appModel3);
            return Ok (appResponseCollection);
        }
    }
}
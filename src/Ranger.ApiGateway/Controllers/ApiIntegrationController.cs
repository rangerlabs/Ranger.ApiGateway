using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ranger.ApiGateway;

namespace Ranger.ApiGateway {

    [ApiController]
    [Authorize (Roles = "User")]
    public class ApiIntegrationController : IntegrationBaseController<ApiIntegrationApiResponseModel> {
        [HttpGet ("/integration/api")]
        public async Task<IActionResult> Index (string name) {
            IActionResult response = new StatusCodeResult (202);
            var appModel = new ApiIntegrationApiResponseModel () {
                Name = "TestApp",
                Description = "This is a test app",
                HttpEndpoint = "http://someapi.com/",
                AuthKey = Guid.NewGuid ().ToString ()
            };
            return Ok (appModel);
        }

        [HttpGet ("/integration/api/all")]
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
            appResponseCollection.Add (appModel1);
            appResponseCollection.Add (appModel2);
            return Ok (appResponseCollection);
        }

        [HttpPost ("/integration/api")]
        public async Task<IActionResult> Post (ApiIntegrationModel apiIntegrationModel) {
            if (apiIntegrationModel == null) {
                throw new ArgumentNullException (nameof (apiIntegrationModel));
            }
            var apiIntegrationResponseModel = new ApiIntegrationApiResponseModel () {
                AppName = apiIntegrationModel.AppName,
                Name = apiIntegrationModel.Name,
                Description = apiIntegrationModel.Description,
                HttpEndpoint = apiIntegrationModel.HttpEndpoint,
                AuthKey = apiIntegrationModel.AuthKey
            };
            return Created ("/integration/api", apiIntegrationResponseModel);
        }
    }

}
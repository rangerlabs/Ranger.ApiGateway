using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ranger.ApiGateway;

namespace Ranger.ApiGateway
{

    [ApiController]
    [Authorize(Roles = "User")]
    public class ApiIntegrationController : IntegrationBaseController<ApiIntegrationApiResponseModel>
    {
        [HttpGet("/integration/api")]
        public async Task<IActionResult> Index(string name)
        {
            IActionResult response = new StatusCodeResult(202);
            var appModel = new ApiIntegrationApiResponseModel()
            {
                Name = "TestApp",
                Description = "This is a test app",
                HttpEndpoint = "http://someapi.com/",
                AuthKey = Guid.NewGuid().ToString()
            };
            return Ok(appModel);
        }

        [HttpGet("/integration/api/all")]
        public async Task<IActionResult> All(string email)
        {
            return Ok();
        }

        [HttpPost("/integration/api")]
        public async Task<IActionResult> Post(ApiIntegrationModel apiIntegrationModel)
        {
            if (apiIntegrationModel == null)
            {
                throw new ArgumentNullException(nameof(apiIntegrationModel));
            }
            var apiIntegrationResponseModel = new ApiIntegrationApiResponseModel()
            {
                Id = apiIntegrationModel.Id,
                AppId = apiIntegrationModel.AppId,
                Name = apiIntegrationModel.Name,
                Description = apiIntegrationModel.Description,
                HttpEndpoint = apiIntegrationModel.HttpEndpoint,
                AuthKey = apiIntegrationModel.AuthKey
            };
            return Created("/integration/api", apiIntegrationResponseModel);
        }
    }
}
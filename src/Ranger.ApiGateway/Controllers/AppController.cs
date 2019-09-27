using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ranger.ApiGateway;
using Ranger.ApiUtilities;

//TODO: These are stubbed out for building the frontend
namespace Ranger.ApiGateway
{
    [ApiController]
    [Authorize(Roles = "User")]
    [TenantDomainRequired]
    public class AppController : ControllerBase
    {

        [HttpGet("/app/{name}")]
        public async Task<IActionResult> Index(string name)
        {
            IActionResult response = new StatusCodeResult(202);
            var appModel = new ApplicationApiResponseModel()
            {
                Name = "TestApp",
                Description = "This is a test app",
                ApiKey = Guid.NewGuid().ToString()
            };
            return Ok(appModel);
        }

        [HttpGet("/app/all")]
        public async Task<IActionResult> All()
        {
            IActionResult response = new StatusCodeResult(202);
            var appResponseCollection = new List<ApplicationApiResponseModel>();
            var appModel1 = new ApplicationApiResponseModel()
            {
                Id = "APP_ID_1",
                Name = "TestApp",
                Description = "This is a test app",
                ApiKey = Guid.NewGuid().ToString()
            };
            var appModel2 = new ApplicationApiResponseModel()
            {
                Id = "APP_ID_2",
                Name = "TestApp2",
                Description = "This is another test app",
                ApiKey = Guid.NewGuid().ToString()
            };
            appResponseCollection.Add(appModel1);
            appResponseCollection.Add(appModel2);
            return Ok(appResponseCollection);
        }

        [HttpPost("app")]
        public async Task<IActionResult> Post(ApplicationModel applicationModel)
        {
            if (applicationModel == null)
            {
                throw new ArgumentNullException(nameof(applicationModel));
            }
            var applicationApiResponseModel = new ApplicationApiResponseModel()
            {
                Name = applicationModel.Name,
                Description = applicationModel.Description
            };
            return Created("", applicationApiResponseModel);
        }
    }
}
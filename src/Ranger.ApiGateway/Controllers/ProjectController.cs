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
    public class ProjectController : ControllerBase
    {

        [HttpGet("/project/{name}")]
        public async Task<IActionResult> Index(string name)
        {
            IActionResult response = new StatusCodeResult(202);
            var projectModel = new ProjectResponseModel()
            {
                Name = "TestApp",
                Description = "This is a test app",
                ApiKey = Guid.NewGuid().ToString()
            };
            return Ok(projectModel);
        }

        [HttpGet("/project/all")]
        public async Task<IActionResult> All()
        {
            IActionResult response = new StatusCodeResult(202);
            var projectResponseCollection = new List<ProjectResponseModel>();
            var projectModel1 = new ProjectResponseModel()
            {
                Name = "APP_ID_1",
                Description = "This is a test app",
                ApiKey = Guid.NewGuid().ToString()
            };
            var projectModel2 = new ProjectResponseModel()
            {
                Name = "APP_ID_2",
                Description = "This is another test app",
                ApiKey = Guid.NewGuid().ToString()
            };
            projectResponseCollection.Add(projectModel1);
            projectResponseCollection.Add(projectModel2);
            return Ok(projectResponseCollection);
        }

        [HttpPost("project")]
        public async Task<IActionResult> Post(ProjectModel projectModel)
        {
            if (projectModel == null)
            {
                throw new ArgumentNullException(nameof(projectModel));
            }
            var projectApiResponseModel = new ProjectResponseModel()
            {
                Name = projectModel.Name,
                Description = projectModel.Description
            };
            return Created("", projectApiResponseModel);
        }
    }
}
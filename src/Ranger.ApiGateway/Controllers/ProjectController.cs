using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.ApiUtilities;
using Ranger.InternalHttpClient;
using Ranger.Common;

//TODO: These are stubbed out for building the frontend
namespace Ranger.ApiGateway
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [TenantDomainRequired]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectsClient projectsClient;
        private readonly ILogger<ProjectController> logger;
        public ProjectController(IProjectsClient projectsClient, ILogger<ProjectController> logger)
        {
            this.logger = logger;
            this.projectsClient = projectsClient;
        }

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
            var domain = HttpContext.Request.Headers.GetPreviouslyVerifiedTenantHeader();

            var request = new { Name = projectModel.Name, Description = projectModel.Description, UserEmail = User.UserFromClaims().Email };

            ProjectResponseModel response = null;
            try
            {
                response = await projectsClient.PostProjectAsync<ProjectResponseModel>(domain, JsonConvert.SerializeObject(request));
            }
            catch (HttpClientException ex)
            {
                logger.LogError("Failed to post project '{projectName}' for domain '{domain}'. The Projects Service responded with code '{code}'.", projectModel.Name, domain, ex.ApiResponse.StatusCode);
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status409Conflict)
                {
                    return Conflict(new { error = $"A project named ${projectModel.Name} already exists." });
                }
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Created("project", response);
        }
    }
}
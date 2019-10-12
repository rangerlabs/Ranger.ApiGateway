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
using System.Net.Http;

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
            var domain = HttpContext.Request.Headers.GetPreviouslyVerifiedTenantHeader();
            var projects = await projectsClient.GetAllProjectsAsync<IEnumerable<ProjectResponseModel>>(domain);
            return Ok(projects);
        }

        [HttpPut("/project/{projectId}")]
        public async Task<IActionResult> Put([FromRoute]string projectId, ProjectModel projectModel)
        {
            var domain = HttpContext.Request.Headers.GetPreviouslyVerifiedTenantHeader();
            var request = new { Name = projectModel.Name, Description = projectModel.Description, Version = projectModel.Version, UserEmail = User.UserFromClaims().Email };

            ProjectResponseModel response = null;
            try
            {
                response = await projectsClient.SendProjectAsync<ProjectResponseModel>(HttpMethod.Put, domain, JsonConvert.SerializeObject(request));
            }
            catch (HttpClientException ex)
            {
                logger.LogError("Failed to post project '{projectName}' for domain '{domain}'. The Projects Service responded with code '{code}'.", projectModel.Name, domain, ex.ApiResponse.StatusCode);
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status409Conflict)
                {
                    var errors = new ApiErrorContent();
                    errors.Errors.Add($"A project named ${projectModel.Name} already exists.");
                    return Conflict(errors);
                }
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Created("project", response);

        }

        [HttpPost("project")]
        public async Task<IActionResult> Post(ProjectModel projectModel)
        {
            var domain = HttpContext.Request.Headers.GetPreviouslyVerifiedTenantHeader();

            var request = new { Name = projectModel.Name, Description = projectModel.Description, UserEmail = User.UserFromClaims().Email };

            ProjectResponseModel response = null;
            try
            {
                response = await projectsClient.SendProjectAsync<ProjectResponseModel>(HttpMethod.Post, domain, JsonConvert.SerializeObject(request));
            }
            catch (HttpClientException<ProjectResponseModel> ex)
            {
                logger.LogError("Failed to post project '{projectName}' for domain '{domain}'. The Projects Service responded with code '{code}'.", projectModel.Name, domain, ex.ApiResponse.StatusCode);
                var errors = new ApiErrorContent();
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status409Conflict)
                {
                    return Conflict(ex.ApiResponse.Errors);
                }
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Created("project", response);
        }
    }
}
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
    [ApiVersion("1.0")]
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

        [HttpGet("/project/all")]
        public async Task<IActionResult> All()
        {
            var domain = HttpContext.Request.Headers.GetPreviouslyVerifiedTenantHeader();
            try
            {
                var projects = await projectsClient.GetAllProjectsAsync<IEnumerable<ProjectResponseModel>>(domain);
                return Ok(projects);
            }
            catch (HttpClientException<IEnumerable<ProjectResponseModel>> ex)
            {
                logger.LogError(ex, "Failed to retrieve projects.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("/project/{projectId}")]
        public async Task<IActionResult> Put([FromRoute]string projectId, PutProjectModel projectModel)
        {
            if (string.IsNullOrWhiteSpace(projectId) || !Guid.TryParse(projectId, out _))
            {
                var errors = new ApiErrorContent();
                errors.Errors.Add($"Invalid project id format.");
                return BadRequest(errors);
            }

            var domain = HttpContext.Request.Headers.GetPreviouslyVerifiedTenantHeader();
            var request = new { Name = projectModel.Name, Description = projectModel.Description, Enabled = projectModel.Enabled, Version = projectModel.Version, UserEmail = User.UserFromClaims().Email };

            ProjectResponseModel response = null;
            try
            {
                response = await projectsClient.PutProjectAsync<ProjectResponseModel>(domain, projectId, JsonConvert.SerializeObject(request));
            }
            catch (HttpClientException<ProjectResponseModel> ex)
            {
                logger.LogError(ex, "Failed to put project '{projectName}' for domain '{domain}'. The Projects Service responded with code '{code}'.", projectModel.Name, domain, ex.ApiResponse.StatusCode);
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status409Conflict)
                {
                    return Conflict(ex.ApiResponse.Errors);
                }
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status304NotModified)
                {
                    var errors = new ApiErrorContent();
                    errors.Errors.Add($"No changes were made to project.");
                    return new ContentResult()
                    {
                        StatusCode = StatusCodes.Status304NotModified,
                        Content = JsonConvert.SerializeObject(errors),
                        ContentType = "application/json",
                    };
                }
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Created("project", response);
        }

        [HttpPost("/project")]
        public async Task<IActionResult> Post(PostProjectModel projectModel)
        {
            var domain = HttpContext.Request.Headers.GetPreviouslyVerifiedTenantHeader();

            var request = new { Name = projectModel.Name, Description = projectModel.Description, Enabled = projectModel.Enabled, UserEmail = User.UserFromClaims().Email };

            ProjectResponseModel response = null;
            try
            {
                response = await projectsClient.PostProjectAsync<ProjectResponseModel>(domain, JsonConvert.SerializeObject(request));
            }
            catch (HttpClientException<ProjectResponseModel> ex)
            {
                logger.LogError(ex, "Failed to post project '{projectName}' for domain '{domain}'. The Projects Service responded with code '{code}'.", projectModel.Name, domain, ex.ApiResponse.StatusCode);
                var errors = new ApiErrorContent();
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status409Conflict)
                {
                    return Conflict(ex.ApiResponse.Errors);
                }
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Created("project", response);
        }

        [HttpDelete("/project/{projectId}")]
        public async Task<IActionResult> SoftDeleteProject([FromRoute]string projectId)
        {
            var domain = HttpContext.Request.Headers.GetPreviouslyVerifiedTenantHeader();
            try
            {
                await projectsClient.SoftDeleteProjectAsync(domain, projectId, User.UserFromClaims().Email);
            }
            catch (HttpClientException<ProjectResponseModel> ex)
            {
                logger.LogError(ex, $"Failed to project with ProjectId '{projectId}'");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return NoContent();
        }

        [HttpPut("/project/{projectId}/{environment}/reset")]
        public async Task<IActionResult> ApiKeyReset([FromRoute]string projectId, [FromRoute]string environment, ApiKeyResetModel apiKeyResetModel)
        {
            var domain = HttpContext.Request.Headers.GetPreviouslyVerifiedTenantHeader();
            var request = new { Version = apiKeyResetModel.Version, UserEmail = User.UserFromClaims().Email };
            ProjectResponseModel response = null;
            try
            {
                response = await projectsClient.ApiKeyResetAsync<ProjectResponseModel>(domain, projectId, environment, JsonConvert.SerializeObject(request));
            }
            catch (HttpClientException<ProjectResponseModel> ex)
            {
                logger.LogError(ex, $"Failed to reset '{environment}' API key for project with ProjectId '{projectId}'");
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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.InternalHttpClient;
using Ranger.Common;
using System.Linq;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    public class ProjectController : BaseController<ProjectController>
    {
        private readonly IBusPublisher busPublisher;
        private readonly IProjectsClient projectsClient;
        private readonly ILogger<ProjectController> logger;

        public ProjectController(IBusPublisher busPublisher, IProjectsClient projectsClient, ILogger<ProjectController> logger) : base(busPublisher, logger)
        {
            this.logger = logger;
            this.busPublisher = busPublisher;
            this.projectsClient = projectsClient;
        }

        [HttpGet("/projects")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetAllProjects()
        {
            try
            {
                var projects = await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectResponseModel>>(UserFromClaims.Domain, UserFromClaims.Email);
                if (projects.Count() > 0)
                {
                    return Ok(projects);
                }
                else
                {
                    return NoContent();
                }
            }
            catch (HttpClientException<IEnumerable<ProjectResponseModel>> ex)
            {
                logger.LogError(ex, "Failed to retrieve projects.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("/projects/{projectId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProject([FromRoute]Guid projectId, PutProjectModel projectModel)
        {
            var user = UserFromClaims;
            var request = new { Name = projectModel.Name, Description = projectModel.Description, Enabled = projectModel.Enabled, Version = projectModel.Version, UserEmail = user.Email };

            ProjectResponseModel response = null;
            try
            {
                response = await projectsClient.PutProjectAsync<ProjectResponseModel>(UserFromClaims.Domain, projectId, JsonConvert.SerializeObject(request));
            }
            catch (HttpClientException<ProjectResponseModel> ex)
            {
                logger.LogError(ex, $"Failed to put project '{projectModel.Name}' for domain '{UserFromClaims.Domain}'. The Projects Service responded with code '{ex.ApiResponse.StatusCode}'.", projectModel.Name, UserFromClaims.Domain, ex.ApiResponse.StatusCode);
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

        [HttpPost("/projects")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProject(PostProjectModel projectModel)
        {
            var user = UserFromClaims;
            var request = new { Name = projectModel.Name, Description = projectModel.Description, Enabled = projectModel.Enabled, UserEmail = user.Email };

            ProjectResponseModel response = null;
            try
            {
                response = await projectsClient.PostProjectAsync<ProjectResponseModel>(UserFromClaims.Domain, JsonConvert.SerializeObject(request));
            }
            catch (HttpClientException<ProjectResponseModel> ex)
            {
                logger.LogError(ex, $"Failed to post project '{projectModel.Name}' for domain '{UserFromClaims.Domain}'. The Projects Service responded with code '{ex.ApiResponse.StatusCode}'.", projectModel.Name, UserFromClaims.Domain, ex.ApiResponse.StatusCode);
                var errors = new ApiErrorContent();
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status409Conflict)
                {
                    return Conflict(ex.ApiResponse.Errors);
                }
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Created("project", response);
        }

        [HttpDelete("/projects/{projectId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDeleteProject([FromRoute]Guid projectId)
        {
            try
            {
                var user = UserFromClaims;
                await projectsClient.SoftDeleteProjectAsync(UserFromClaims.Domain, projectId, user.Email);
            }
            catch (HttpClientException<ProjectResponseModel> ex)
            {
                logger.LogError(ex, $"Failed to project with ProjectId '{projectId}'");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return NoContent();
        }

        [HttpPut("/projects/{projectId}/{environment}/reset")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApiKeyReset([FromRoute]Guid projectId, [FromRoute]string environment, ApiKeyResetModel apiKeyResetModel)
        {
            var user = UserFromClaims;
            var request = new { Version = apiKeyResetModel.Version, UserEmail = user.Email };
            ProjectResponseModel response = null;
            try
            {
                response = await projectsClient.ApiKeyResetAsync<ProjectResponseModel>(UserFromClaims.Domain, projectId, environment, JsonConvert.SerializeObject(request));
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
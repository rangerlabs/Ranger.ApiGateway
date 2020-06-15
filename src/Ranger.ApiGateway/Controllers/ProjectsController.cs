using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Policy = "TenantIdResolved")]
    public class ProjectsController : BaseController<ProjectsController>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ProjectsHttpClient projectsClient;
        private readonly ILogger<ProjectsController> logger;

        public ProjectsController(IBusPublisher busPublisher, ProjectsHttpClient projectsClient, ILogger<ProjectsController> logger) : base(busPublisher, logger)
        {
            this.logger = logger;
            this.busPublisher = busPublisher;
            this.projectsClient = projectsClient;
        }

        ///<summary>
        /// Gets all projects
        ///</summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("/projects")]
        [Authorize(Roles = "User")]
        public async Task<ApiResponse> GetAllProjects()
        {
            var apiResponse = await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectResponseModel>>(TenantId, UserFromClaims.Email);
            return new ApiResponse("Successfully retrieved all projects", apiResponse.Result);
        }

        ///<summary>
        /// Updates an existing project
        ///</summary>
        ///<param name="projectId">The project's unique identifier</param>
        ///<param name="projectModel">The project model necessary to update an existing project</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpPut("/projects/{projectId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> UpdateProject([FromRoute] Guid projectId, PutProjectModel projectModel)
        {
            var request = new { Name = projectModel.Name, Description = projectModel.Description, Enabled = projectModel.Enabled, Version = projectModel.Version, UserEmail = UserFromClaims.Email };
            var apiResponse = await projectsClient.PutProjectAsync<ProjectResponseModel>(TenantId, projectId, JsonConvert.SerializeObject(request));
            return new ApiResponse("Successfully updated project", apiResponse.Result);
        }

        ///<summary>
        /// Creates a new project
        ///</summary>
        ///<param name="projectModel">The project model necessary to create a new project</param>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost("/projects")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> CreateProject(PostProjectModel projectModel)
        {
            var request = new { Name = projectModel.Name, Description = projectModel.Description, Enabled = projectModel.Enabled, UserEmail = UserFromClaims.Email };
            var apiResponse = await projectsClient.PostProjectAsync<ProjectResponseModel>(TenantId, JsonConvert.SerializeObject(request));
            return new ApiResponse($"Successfully created project '{projectModel.Name}'", apiResponse.Result);
        }

        ///<summary>
        /// Deletes a project
        ///</summary>
        ///<param name="projectId">The project's unique identifier</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpDelete("/projects/{projectId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> SoftDeleteProject(Guid projectId)
        {
            var apiResponse = await projectsClient.SoftDeleteProjectAsync(TenantId, projectId, UserFromClaims.Email);
            return new ApiResponse("Successfully deleted project", apiResponse.Result);
        }

        ///<summary>
        /// Resets a project environment's API key
        ///</summary>
        ///<param name="projectId">The project's unique identifier</param>
        ///<param name="purpose">The environment whose API key to reset, 'live' or 'test'</param>
        ///<param name="apiKeyResetModel">The model necessary to reset an API key</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpPut("/projects/{projectId}/{environment}/reset")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> ApiKeyReset(Guid projectId, ApiKeyPurposeEnum purpose, ApiKeyResetModel apiKeyResetModel)
        {
            var request = new { Version = apiKeyResetModel.Version, UserEmail = UserFromClaims.Email };
            var apiResponse = await projectsClient.ApiKeyResetAsync<ProjectResponseModel>(TenantId, projectId, purpose, JsonConvert.SerializeObject(request));
            return new ApiResponse("Successfully reset API key", apiResponse.Result);
        }
    }
}
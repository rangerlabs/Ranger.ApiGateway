using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    public class IntegrationsController : BaseController<IntegrationsController>
    {
        private readonly IntegrationsHttpClient integrationsClient;
        private readonly ILogger<IntegrationsController> logger;
        private readonly ProjectsHttpClient projectsClient;

        public IntegrationsController(IBusPublisher busPublisher, IntegrationsHttpClient integrationsClient, ProjectsHttpClient projectsClient, ILogger<IntegrationsController> logger) : base(busPublisher, logger)
        {
            this.projectsClient = projectsClient;
            this.logger = logger;
            this.integrationsClient = integrationsClient;
        }

        ///<summary>
        /// Deletes an existing integration within a project
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        ///<param name="integrationName">The friendly name of the integration to delete</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpDelete("/{projectId}/integrations/{integrationName}")]
        [Authorize(Roles = "Admin")]
        [Authorize(Policy = AuthorizationPolicyNames.BelongsToProject)]
        public async Task<ApiResponse> DeleteIntegrationForProject(Guid projectId, string integrationName)
        {
            return await Task.Run(() => base.SendAndAccept(new DeleteIntegrationSagaInitializer(UserFromClaims.Email, TenantId, integrationName, projectId)));
        }
        ///<summary>
        /// Deletes an existing integration within a project
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/{projectId}/integrations")]
        [Authorize(Roles = "User")]
        [Authorize(Policy = AuthorizationPolicyNames.BelongsToProject)]
        public async Task<ApiResponse> GetAllIntegrationsForProject(Guid projectId)
        {
            var integrationsApiResponse = await integrationsClient.GetAllIntegrationsByProjectId<IEnumerable<dynamic>>(TenantId, projectId);
            return new ApiResponse("Succesfully retrieved integrations", integrationsApiResponse.Result);
        }
    }
}
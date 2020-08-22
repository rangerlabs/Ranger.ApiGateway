using System;
using System.Collections.Generic;
using System.Threading;
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
    public class IntegrationsUserController : IntegrationsBaseController
    {
        public IntegrationsUserController(IBusPublisher busPublisher, IntegrationsHttpClient integrationsClient, ProjectsHttpClient projectsClient, ILogger<IntegrationsUserController> logger)
        : base(busPublisher, integrationsClient, projectsClient, logger)
        { }

        ///<summary>
        /// Deletes an existing integration within a project
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        ///<param name="integrationName">The friendly name of the integration to delete</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpDelete("/{projectId}/integrations/{integrationName}")]
        [Authorize(Roles = "Admin")]
        [Authorize(Policy = AuthorizationPolicyNames.BelongsToProject)]
        public async Task<ApiResponse> DeleteIntegrationForProjectByUser(Guid projectId, string integrationName)
        {
            return await base.DeleteIntegrationForProject(projectId, integrationName);
        }
        ///<summary>
        /// Deletes an existing integration within a project
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/{projectId}/integrations")]
        [Authorize(Roles = "User")]
        [Authorize(Policy = AuthorizationPolicyNames.BelongsToProject)]
        public async Task<ApiResponse> GetAllIntegrationsForProjectByUser(Guid projectId, CancellationToken cancellationToken)
        {
            return await base.GetAllIntegrationsForProject(projectId, cancellationToken);
        }
    }
}
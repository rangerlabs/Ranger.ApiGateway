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
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    public class IntegrationsProjectApiKeyController : IntegrationsBaseController
    {
        public IntegrationsProjectApiKeyController(IBusPublisher busPublisher, IIntegrationsHttpClient integrationsClient, IProjectsHttpClient projectsClient, ILogger<IntegrationsProjectApiKeyController> logger)
        : base(busPublisher, integrationsClient, projectsClient, logger)
        { }

        ///<summary>
        /// Retrieves all integrations for a project
        ///</summary>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/integrations")]
        [Authorize(Policy = AuthorizationPolicyNames.ValidProjectApiKey)]
        public async Task<ApiResponse> GetAllIntegrationsForProject(CancellationToken cancellationToken)
        {
            return await base.GetAllIntegrationsForProject(ProjectId, cancellationToken);
        }

        private Guid ProjectId => (HttpContext.Items[HttpContextAuthItems.Project] as ProjectModel).Id;
    }
}
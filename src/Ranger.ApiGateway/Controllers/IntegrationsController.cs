using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
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
        ///<param name="projectName">The friendly name of the project</param>
        ///<param name="integrationName">The friendly name of the integration to delete</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpDelete("{projectName}/integrations/{integrationName}")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> DeleteIntegrationForProject(string projectName, string integrationName)
        {
            var project = HttpContext.Items["AutorizedProject"] as ProjectModel;
            return await Task.Run(() => base.Send(new DeleteIntegrationSagaInitializer(UserFromClaims.Email, UserFromClaims.Domain, integrationName, project.ProjectId)));
        }
        ///<summary>
        /// Deletes an existing integration within a project
        ///</summary>
        ///<param name="projectName">The friendly name of the project</param>
        [HttpGet("{projectName}/integrations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "User")]
        [Authorize("BelongsToProject")]
        public async Task<ApiResponse> GetAllIntegrationsForProject(string projectName)
        {
            var project = HttpContext.Items["AuthorizedProject"] as ProjectModel;
            var integrations = await integrationsClient.GetAllIntegrationsByProjectId<IEnumerable<dynamic>>(UserFromClaims.Domain, project.ProjectId);
            return new ApiResponse("Succesfully retrived integrations", integrations);
        }
    }
}
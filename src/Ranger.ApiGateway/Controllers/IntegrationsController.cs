using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    [Authorize(Roles = "User")]
    [Route("{projectName}/integrations")]
    public class IntegrationsController : BaseController<IntegrationsController>
    {
        private readonly IIntegrationsClient integrationsClient;
        private readonly ILogger<IntegrationsController> logger;
        private readonly IProjectsClient projectsClient;

        public IntegrationsController(IBusPublisher busPublisher, IIntegrationsClient integrationsClient, IProjectsClient projectsClient, ILogger<IntegrationsController> logger) : base(busPublisher, logger)
        {
            this.projectsClient = projectsClient;
            this.logger = logger;
            this.integrationsClient = integrationsClient;
        }

        [HttpDelete("{integrationName}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteIntegrationForProject(string projectName, [FromRoute] string integrationName)
        {
            var projectId = (await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(UserFromClaims.Domain, UserFromClaims.Email)).FirstOrDefault(_ => _.Name == projectName)?.ProjectId;
            if (!(projectId is null) || !projectId.Equals(Guid.Empty))
            {
                return await Task.Run(() => base.Send(new DeleteIntegrationSagaInitializer(UserFromClaims.Email, UserFromClaims.Domain, integrationName, projectId.GetValueOrDefault())));
            }
            logger.LogWarning("The user was authorized for a project but the project ID was not successfully retrieved.");
            return NotFound();
        }

        [HttpGet]
        [Authorize("BelongsToProject")]
        public async Task<IActionResult> GetAllIntegrationsForProject(string projectName)
        {
            try
            {
                var projectId = (await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(UserFromClaims.Domain, UserFromClaims.Email)).FirstOrDefault(_ => _.Name == projectName)?.ProjectId;
                if (!(projectId is null) || !projectId.Equals(Guid.Empty))
                {
                    var integrations = await integrationsClient.GetAllIntegrationsByProjectId<IEnumerable<dynamic>>(UserFromClaims.Domain, projectId.GetValueOrDefault());
                    if (integrations.Count() > 0)
                    {
                        return Ok(integrations);
                    }
                    else
                    {
                        return NoContent();
                    }
                }
                logger.LogWarning("The user was authorized for a project but the project ID was not successfully retrieved.");
                return NotFound();
            }
            catch (HttpClientException<IEnumerable<dynamic>> ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve integrations.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
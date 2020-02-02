using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.ApiUtilities;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "User")]
    [TenantDomainRequired]
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

        [HttpGet]
        [Authorize("BelongsToProject")]
        public async Task<IActionResult> GetAllIntegrationsForProject(string projectName)
        {
            try
            {
                var projectId = (await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(Domain, User.UserFromClaims().Email)).FirstOrDefault(_ => _.Name == projectName)?.ProjectId;
                if (!(projectId is null) || !projectId.Equals(Guid.Empty))
                {
                    var geofences = await integrationsClient.GetAllIntegrationsByProjectId<IEnumerable<dynamic>>(Domain, projectId.GetValueOrDefault());
                    if (geofences.Count() > 0)
                    {
                        return Ok(geofences);
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
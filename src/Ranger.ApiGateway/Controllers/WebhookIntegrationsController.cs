using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.ApiGateway;
using Ranger.ApiGateway.Messages.Commands;
using Ranger.ApiUtilities;
using Ranger.Common;
using Ranger.Common.SharedKernel;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{

    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "User")]
    [TenantDomainRequired]
    [Route("{projectName}/integrations/webhook")]
    public class WebhookIntegrationController : BaseController<WebhookIntegrationController>
    {
        private readonly IBusPublisher busPublisher;
        private readonly IProjectsClient projectsClient;
        private readonly ILogger<WebhookIntegrationController> logger;

        public WebhookIntegrationController(IBusPublisher busPublisher, IProjectsClient projectsClient, ILogger<WebhookIntegrationController> logger) : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
            this.projectsClient = projectsClient;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromRoute]string projectName, [FromBody]WebhookIntegrationPostModel webhookIntegrationModel)
        {
            try
            {
                var projectId = (await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(Domain, User.UserFromClaims().Email)).FirstOrDefault(_ => _.Name == projectName)?.ProjectId;
                if (!(projectId is null) || !projectId.Equals(Guid.Empty))
                {
                    var createIntegrationSagaInitializer = new CreateIntegrationSagaInitializer(
                        User?.UserFromClaims().Email ?? "", //INSERT TOKEN HERE
                        Domain,
                        webhookIntegrationModel.Name,
                        projectId.GetValueOrDefault(),
                        JsonConvert.SerializeObject(webhookIntegrationModel),
                        IntegrationsEnum.WEBHOOK
                    );
                    return await Task.Run(() => base.Send(createIntegrationSagaInitializer));
                }
                else
                {
                    logger.LogWarning("The user was authorized for a project but the project ID was not successfully retrieved. This may be the result of a concurrency issue. Verify the project still exists.");
                    return NotFound();
                }
            }
            catch (HttpClientException<IEnumerable<GeofenceResponseModel>> ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create integration.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
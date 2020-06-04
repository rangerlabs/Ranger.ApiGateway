using System;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ranger.ApiGateway.Messages.Commands;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{

    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Authorize(Policy = "BelongsToProject")]
    public class WebhookIntegrationController : BaseController<WebhookIntegrationController>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ProjectsHttpClient projectsClient;
        private readonly ILogger<WebhookIntegrationController> logger;

        public WebhookIntegrationController(IBusPublisher busPublisher, ProjectsHttpClient projectsClient, ILogger<WebhookIntegrationController> logger) : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
            this.projectsClient = projectsClient;
            this.logger = logger;
        }

        ///<summary>
        /// Creates a new WebHook Integration
        ///</summary>
        ///<param name="projectName">The friendly name of the project</param>
        ///<param name="webhookIntegrationModel">The model necessary to create a new webhook integration</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpPost("/{projectName}/integrations/webhook")]
        public async Task<ApiResponse> Post(string projectName, WebhookIntegrationPostModel webhookIntegrationModel)
        {
            var project = HttpContext.Items["AuthorizedProject"] as ProjectModel;
            var createIntegrationSagaInitializer = new CreateIntegrationSagaInitializer(
                 UserFromClaims.Email ?? "", //INSERT TOKEN HERE
                 TenantId,
                 webhookIntegrationModel.Name,
                 project.ProjectId,
                 JsonConvert.SerializeObject(webhookIntegrationModel),
                 IntegrationsEnum.WEBHOOK
             );
            return await Task.Run(() => base.SendAndAccept(createIntegrationSagaInitializer));
        }

        ///<summary>
        /// Creates a new WebHook Integration
        ///</summary>
        ///<param name="projectName">The friendly name of the project</param>
        ///<param name="id">The integration id to update</param>
        ///<param name="webhookIntegrationModel">The model necessary to create a new webhook integration</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpPut("/{projectName}/integrations/webhook/{id}")]
        public async Task<ApiResponse> UpdateIntegration(string projectName, Guid id, WebhookIntegrationPutModel webhookIntegrationModel)
        {
            var project = HttpContext.Items["AuthorizedProject"] as ProjectModel;
            webhookIntegrationModel.IntegrationId = id;
            var updateIntegrationSagaInitializer = new UpdateIntegrationSagaInitializer(
                UserFromClaims.Email,
                TenantId,
                webhookIntegrationModel.Name,
                project.ProjectId,
                JsonConvert.SerializeObject(webhookIntegrationModel),
                IntegrationsEnum.WEBHOOK,
                webhookIntegrationModel.Version
            );
            return await Task.Run(() => base.SendAndAccept(updateIntegrationSagaInitializer));
        }
    }
}
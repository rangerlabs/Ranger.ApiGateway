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
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.ApiGateway
{

    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Authorize(Policy = AuthorizationPolicyNames.BelongsToProject)]
    public class PusherIntegrationsController : BaseController
    {
        private readonly IBusPublisher busPublisher;
        private readonly IProjectsHttpClient projectsClient;
        private readonly ILogger<PusherIntegrationsController> logger;

        public PusherIntegrationsController(IBusPublisher busPublisher, IProjectsHttpClient projectsClient, ILogger<PusherIntegrationsController> logger) : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
            this.projectsClient = projectsClient;
            this.logger = logger;
        }

        ///<summary>
        /// Creates a new Pusher Integration
        ///</summary>
        ///<param name="projectId">The friendly name of the project</param>
        ///<param name="pusherIntegrationModel">The model necessary to create a new pusher integration</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpPost("/{projectId}/integrations/pusher")]
        public async Task<ApiResponse> Post(Guid projectId, PusherIntegrationPostModel pusherIntegrationModel)
        {
            pusherIntegrationModel.Name = pusherIntegrationModel.Name.Trim();
            var createIntegrationSagaInitializer = new CreateIntegrationSagaInitializer(
                 UserFromClaims.Email ?? "", //INSERT TOKEN HERE
                 TenantId,
                 pusherIntegrationModel.Name,
                 projectId,
                 JsonConvert.SerializeObject(pusherIntegrationModel),
                 IntegrationsEnum.PUSHER
             );
            return await Task.Run(() => base.SendAndAccept(createIntegrationSagaInitializer));
        }

        ///<summary>
        /// Creates a new Pusher Integration
        ///</summary>
        ///<param name="projectId">The friendly name of the project</param>
        ///<param name="id">The integration id to update</param>
        ///<param name="pusherIntegrationModel">The model necessary to create a new pusher integration</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpPut("/{projectId}/integrations/pusher/{id}")]
        public async Task<ApiResponse> UpdateIntegration(Guid projectId, Guid id, PusherIntegrationPutModel pusherIntegrationModel)
        {
            // The IntegrationId property on the model is not required for the endpoint but is required of the serialized object for the Integrations service
            // Overriding any IntegrationId in the body with what was passed in the path allows this endpoint to still be RESTful
            pusherIntegrationModel.Id = id;
            pusherIntegrationModel.Name = pusherIntegrationModel.Name.Trim();
            var updateIntegrationSagaInitializer = new UpdateIntegrationSagaInitializer(
                UserFromClaims.Email,
                TenantId,
                pusherIntegrationModel.Name,
                projectId,
                JsonConvert.SerializeObject(pusherIntegrationModel),
                IntegrationsEnum.PUSHER,
                pusherIntegrationModel.Version
            );
            return await Task.Run(() => base.SendAndAccept(updateIntegrationSagaInitializer));
        }
    }
}
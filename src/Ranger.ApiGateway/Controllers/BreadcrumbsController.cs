using System;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.ApiGateway.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Policy = AuthorizationPolicyNames.ValidBreadcrumbApiKey)]
    public class BreadcrumbsController : BaseController
    {
        private readonly IBusPublisher busPublisher;
        private readonly SubscriptionsHttpClient subscriptionsHttpClient;
        private readonly ILogger<BreadcrumbsController> logger;

        public BreadcrumbsController(IBusPublisher busPublisher, SubscriptionsHttpClient subscriptionsHttpClient, ILogger<BreadcrumbsController> logger) : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
            this.subscriptionsHttpClient = subscriptionsHttpClient;
            this.logger = logger;
        }

        ///<summary>
        /// Accepts new breadcrumbs for geofence determination
        ///</summary>
        ///<param name="breadcrumbModel">The model necessary to compute geofence resuts</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpPost("/breadcrumbs")]
        public async Task<ApiResponse> PostBreadcrumb([FromBody] BreadcrumbModel breadcrumbModel)
        {
            if (!(await isSubscriptionActive()))
            {
                throw new ApiException("The subscription is inactive.", statusCode: StatusCodes.Status402PaymentRequired);
            }

            breadcrumbModel.DeviceId = breadcrumbModel.DeviceId.Trim();
            breadcrumbModel.ExternalUserId = breadcrumbModel.ExternalUserId?.Trim();

            logger.LogDebug("Breadcrumb received");
            var environment = Enum.Parse<EnvironmentEnum>(HttpContext.Items[HttpContextAuthItems.BreadcrumbApiKeyEnvironment] as string);
            var project = HttpContext.Items[HttpContextAuthItems.Project] as ProjectModel;
            return base.SendAndAccept(new ComputeGeofenceIntersections(
                    TenantId,
                    project.Id,
                    project.Name,
                    environment,
                    new Breadcrumb(
                        breadcrumbModel.DeviceId,
                        breadcrumbModel.ExternalUserId,
                        breadcrumbModel.Position,
                        breadcrumbModel.RecordedAt.ToUniversalTime(),
                        DateTime.UtcNow,
                        breadcrumbModel.Metadata,
                        breadcrumbModel.Accuracy)
                ),
                clientMessage: "Breadcrumb accepted"
            );
        }

        [NonAction]
        private async Task<bool> isSubscriptionActive()
        {
            var isActiveResponse = await this.subscriptionsHttpClient.IsSubscriptionActive(TenantId);
            return isActiveResponse.Result;
        }
    }
}
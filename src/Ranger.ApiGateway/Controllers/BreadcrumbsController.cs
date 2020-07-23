using System;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Policy = AuthorizationPolicyNames.ValidBreadcrumbApiKey)]
    public class BreadcrumbsController : BaseController<BreadcrumbsController>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<BreadcrumbsController> logger;
        public BreadcrumbsController(IBusPublisher busPublisher, ILogger<BreadcrumbsController> logger) : base(busPublisher, logger)
        {
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        ///<summary>
        /// Accepts new breadcrumbs for geofence determination
        ///</summary>
        ///<param name="breadcrumbModel">The model necessary to compute geofence resuts</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpPost("/breadcrumbs")]
        public ApiResponse PostBreadcrumb([FromBody] BreadcrumbModel breadcrumbModel)
        {
            breadcrumbModel.ExternalUserId = breadcrumbModel.ExternalUserId.Trim();

            logger.LogDebug("Breadcrumb received");
            var environment = Enum.Parse<EnvironmentEnum>(HttpContext.Items[HttpContextAuthItems.BreadcrumbApiKeyEnvironment] as string);
            var project = HttpContext.Items[HttpContextAuthItems.Project] as ProjectModel;
            return base.SendAndAccept(new ComputeGeofenceIntersections(
                        TenantId,
                        project.ProjectId,
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
    }
}
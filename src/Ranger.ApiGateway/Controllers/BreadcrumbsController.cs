using System;
using System.Threading.Tasks;
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
    [Authorize(Policy = "ValidApiKey")]
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
        public async Task<ApiResponse> PostBreadcrumb([FromBody] BreadcrumbModel breadcrumbModel)
        {
            logger.LogDebug("Breadcrumb received");
            var environment = Enum.Parse<EnvironmentEnum>(HttpContext.Items["ApiKeyEnvironment"] as string);
            var tenantId = HttpContext.Items["TenantId"] as string;
            var projectId = Guid.Parse(HttpContext.Items["ProjectId"] as string);
            return await Task.Run(() =>
                base.Send(new ComputeGeofenceIntersections(
                        tenantId,
                        projectId,
                        environment,
                        new Breadcrumb(
                            breadcrumbModel.DeviceId,
                            breadcrumbModel.ExternalUserId,
                            breadcrumbModel.Position,
                            breadcrumbModel.RecordedAt,
                            breadcrumbModel.Metadata,
                            breadcrumbModel.Accuracy)
                        )
                )
            );
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.ApiGateway.Middleware;
using Ranger.ApiGateway.Models.External.BreadcrumbsControllerModels;
using Ranger.Common.SharedKernel;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [ApiKeyRequired]
    public class BreadcrumbsController : BaseController<BreadcrumbsController>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<BreadcrumbsController> logger;
        public BreadcrumbsController(IBusPublisher busPublisher, ILogger<BreadcrumbsController> logger) : base(busPublisher, logger)
        {
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        [HttpPost("/breadcrumbs")]
        public IActionResult PostBreadcrumb([FromBody] Breadcrumb model)
        {
            logger.LogDebug("Breadcrumb received.");
            var environment = Enum.Parse<EnvironmentEnum>(HttpContext.Items["ApiKeyEnvironment"] as string);
            var databaseUsername = HttpContext.Items["DatabaseUsername"] as string;
            var projectId = Guid.Parse(HttpContext.Items["ProjectId"] as string);
            return base.Send(new ComputeGeofenceIntersections(databaseUsername, projectId, environment, model));
        }
    }
}
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
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.ApiGateway.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Policy = AuthorizationPolicyNames.TenantIdResolved)]
    public class TestRunController : BaseController
    {
        private readonly IBusPublisher busPublisher;
        private readonly ProjectsHttpClient projectsClient;
        private readonly ILogger<BreadcrumbsController> logger;

        public TestRunController(IBusPublisher busPublisher, ProjectsHttpClient projectsClient, ILogger<BreadcrumbsController> logger) : base(busPublisher, logger)
        {
            this.logger = logger;
            this.busPublisher = busPublisher;
            this.projectsClient = projectsClient;
        }

        ///<summary>
        /// Accepts a Test Run for geofence determination
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        ///<param name="testRunModel">The model necessary to compute geofence resuts</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("/{projectId}/test-runs")]
        [Authorize(Roles = "User")]
        [Authorize(Policy = AuthorizationPolicyNames.BelongsToProject)]
        public ApiResponse PostBreadcrumb(Guid projectId, [FromBody] TestRunModel testRunModel)
        {
            logger.LogDebug("Test Run received");
            var project = HttpContext.Items[HttpContextAuthItems.Project] as ProjectModel;

            var accuracy = new Random().Next(30);
            var recordedAt = DateTime.UtcNow - TimeSpan.FromMinutes(5) * testRunModel.Positions.Count();
            var breadcrumbs = new List<Breadcrumb>();
            foreach (var position in testRunModel.Positions)
            {
                breadcrumbs.Add(
                    new Breadcrumb(
                        "Ranger_Test_Runner",
                        "Test_User_0",
                        position,
                        recordedAt,
                        DateTime.UtcNow,
                        default,
                        accuracy)
                );
                recordedAt += TimeSpan.FromMinutes(5);
            }
            base.Send(new ExecuteTestRun(TenantId, projectId, project.Name, breadcrumbs));
            return new ApiResponse("The Test Run has been queued and will execute shortly", statusCode: StatusCodes.Status202Accepted);
        }
    }
}
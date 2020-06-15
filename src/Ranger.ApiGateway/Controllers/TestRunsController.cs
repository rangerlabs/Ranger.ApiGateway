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

namespace Ranger.ApiGateway.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Policy = "TenantIdResolved")]
    public class TestRunController : BaseController<BreadcrumbsController>
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
        ///<param name="testRunModel">The model necessary to compute geofence resuts</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("/{projectName}/test-runs")]
        [Authorize(Roles = "User")]
        [Authorize(Policy = "BelongsToProject")]
        public async Task<ApiResponse> PostBreadcrumb([FromBody] TestRunModel testRunModel)
        {
            logger.LogDebug("Test Run received");
            var project = HttpContext.Items["AuthorizedProject"] as ProjectModel;

            await Task.Run(() =>
            {
                var accuracy = new Random().Next(30);
                var recordedAt = DateTime.UtcNow - TimeSpan.FromMinutes(5) * testRunModel.Positions.Count();
                foreach (var position in testRunModel.Positions)
                {
                    base.Send(new ComputeGeofenceIntersections(
                        TenantId,
                        project.ProjectId,
                        project.Name,
                        EnvironmentEnum.TEST,
                        new Breadcrumb(
                            "Ranger_Test_Runner",
                            "Test_User_0",
                            position,
                            recordedAt,
                            default,
                            accuracy)
                    ));
                    recordedAt += TimeSpan.FromMinutes(5);
                }
            });
            return new ApiResponse("Test Run Accepted", statusCode: StatusCodes.Status202Accepted);
        }
    }
}
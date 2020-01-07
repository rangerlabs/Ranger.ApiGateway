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
    public class GeofenceController : BaseController<GeofenceController>
    {
        private readonly IGeofencesClient geofencesClient;
        private readonly ILogger<GeofenceController> logger;
        private readonly IProjectsClient projectsClient;

        public GeofenceController(IBusPublisher busPublisher, IGeofencesClient geofencesClient, IProjectsClient projectsClient, ILogger<GeofenceController> logger) : base(busPublisher, logger)
        {
            this.projectsClient = projectsClient;
            this.logger = logger;
            this.geofencesClient = geofencesClient;
        }

        [HttpGet("/{projectName}/geofences/{name}")]
        public async Task<IActionResult> Index(string projectName, string name)
        {
            return Ok();
        }

        [HttpGet("/{projectName}/geofences")]
        [Authorize("BelongsToProject")]
        public async Task<IActionResult> GetAllGeofences(string projectName)
        {
            try
            {
                var projectId = (await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(Domain, User.UserFromClaims().Email)).FirstOrDefault(_ => _.Name == projectName)?.ProjectId;
                if (!String.IsNullOrWhiteSpace(projectId))
                {
                    var geofences = await geofencesClient.GetAllGeofencesByProjectId<IEnumerable<GeofenceResponseModel>>(Domain, projectId);
                    return Ok(geofences);
                }
                logger.LogWarning("The user was authorized for a project but the project ID was not successfully retrieved.");
                return NotFound();
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
                logger.LogError(ex, "Failed to retrieve geofences.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPost("/{projectName}/geofences")]
        public async Task<IActionResult> Post([FromRoute] string projectName, [FromBody] GeofenceModel geoFenceModel)
        {
            if (geoFenceModel.Shape == GeofenceShapeEnum.Circle)
            {
                if (geoFenceModel.Radius < 50)
                {
                    return BadRequest("Circular geofence radii must be greater than or equal to 50 meters.");
                }
                if (geoFenceModel.Coordinates.Count() == 0 || geoFenceModel.Coordinates.Count() > 1)
                {
                    return BadRequest("Circular geofence must have exactly one coordinate in their Coordinate array.");
                }
            }
            else
            {
                geoFenceModel.Radius = 0;
                if (geoFenceModel.Coordinates.Count() < 3)
                {
                    return BadRequest("Polygon geofence must have three or more coordinate in their Coordinate array.");
                }
            }

            if (geoFenceModel.ExpirationDate < geoFenceModel.LaunchDate)
            {
                return BadRequest("The geofence expiration date was before the launch date.");
            }

            var createGeofenceSagaInitializer = new CreateGeofenceSagaInitializer(
                User is null ? false : true,
                User?.UserFromClaims().Email ?? "", //INSERT TOKEN HERE
                Domain,
                geoFenceModel.ExternalId,
                geoFenceModel.ProjectId,
                geoFenceModel.Shape,
                geoFenceModel.Coordinates,
                geoFenceModel.Labels,
                geoFenceModel.IntegrationIds,
                geoFenceModel.Metadata,
                geoFenceModel.Description,
                geoFenceModel.Radius,
                geoFenceModel.Enabled,
                geoFenceModel.OnEnter,
                geoFenceModel.OnExit,
                geoFenceModel.ExpirationDate,
                geoFenceModel.LaunchDate,
                geoFenceModel.Schedule
            );
            return await Task.Run(() => base.Send(createGeofenceSagaInitializer));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.ApiUtilities;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "User")]
    [TenantDomainRequired]
    public class GeofenceController : BaseController<GeofenceController>
    {
        private readonly IBusPublisher busPublisher;

        public GeofenceController(IBusPublisher busPublisher, ILogger<GeofenceController> logger) : base(busPublisher, logger)
        {
            this.busPublisher = busPublisher;
        }

        [HttpGet("/{projectName}/geofences/{name}")]
        public async Task<IActionResult> Index(string projectName, string name)
        {
            return Ok();
        }

        [HttpGet("/{projectName}/geofences")]
        public async Task<IActionResult> GetAllGeofences(string projectName)
        {
            return Ok(new List<object>());
        }

        [HttpPost("/{projectName}/geofences")]
        public async Task<IActionResult> Post([FromRoute]string projectName, [FromBody]GeofenceModel geoFenceModel)
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
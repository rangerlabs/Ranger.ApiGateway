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

        [HttpGet("/{projectName}/geofences")]
        [Authorize("BelongsToProject")]
        public async Task<IActionResult> GetAllGeofences(string projectName)
        {
            try
            {
                var projectId = (await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(Domain, User.UserFromClaims().Email)).FirstOrDefault(_ => _.Name == projectName)?.ProjectId;
                if (!(projectId is null) || !projectId.Equals(Guid.Empty))
                {
                    var geofences = await geofencesClient.GetAllGeofencesByProjectId<IEnumerable<GeofenceResponseModel>>(Domain, projectId.GetValueOrDefault());
                    if (geofences.Count() > 0)
                    {
                        return Ok(geofences);
                    }
                    else
                    {
                        return NoContent();
                    }
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
                logger.LogError(ex, "Failed to retrieve geofences.");
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve geofences.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPost("/{projectName}/geofences")]
        [Authorize("BelongsToProject")]
        public async Task<IActionResult> Post([FromRoute] string projectName, [FromBody] GeofencePostModel geoFenceModel)
        {
            try
            {
                var projectId = (await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(Domain, User.UserFromClaims().Email)).FirstOrDefault(_ => _.Name == projectName)?.ProjectId;
                if (!(projectId is null) || !projectId.Equals(Guid.Empty))
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
                        if (geoFenceModel.Coordinates.First().Equals(geoFenceModel.Coordinates.Last()))
                        {
                            return BadRequest("The first and last coordinates in a polygon are implicitely connected. Remove the explicit closing edge.");
                        }
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
                        projectId.GetValueOrDefault(),
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
                logger.LogWarning("The user was authorized for a project but the project ID was not successfully retrieved. This may be the result of a concurrency issue. Verify the project still exists.");
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
                logger.LogError(ex, "Failed to create geofence.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPut("/{projectName}/geofences/{id}")]
        [Authorize("BelongsToProject")]
        public async Task<IActionResult> UpdateGeofence([FromRoute] string projectName, [FromRoute] Guid id, [FromBody] GeofencePutModel geoFenceModel)
        {
            try
            {
                var projectId = (await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(Domain, User.UserFromClaims().Email)).FirstOrDefault(_ => _.Name == projectName)?.ProjectId;
                if (!(projectId is null) || !projectId.Equals(Guid.Empty))
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
                        if (geoFenceModel.Coordinates.First().Equals(geoFenceModel.Coordinates.Last()))
                        {
                            return BadRequest("The first and last coordinates in a polygon are implicitely connected. Remove the explicit closing edge.");
                        }
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

                    var createGeofenceSagaInitializer = new UpdateGeofenceSagaInitializer(
                        User is null ? false : true,
                        User?.UserFromClaims().Email ?? "", //INSERT TOKEN HERE
                        Domain,
                        id,
                        geoFenceModel.ExternalId,
                        projectId.GetValueOrDefault(),
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
                logger.LogWarning("The user was authorized for a project but the project ID was not successfully retrieved. This may be the result of a concurrency issue. Verify the project still exists.");
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
                logger.LogError(ex, "Failed to upsert geofence.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpDelete("/{projectName}/geofences/{externalId}")]
        [Authorize("BelongsToProject")]
        public async Task<IActionResult> DeleteGeofence([FromRoute] string projectName, [FromRoute] string externalId)
        {
            try
            {
                var projectId = (await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(Domain, User.UserFromClaims().Email)).FirstOrDefault(_ => _.Name == projectName)?.ProjectId;
                if (!(projectId is null) || !projectId.Equals(Guid.Empty))
                {
                    var deleteGeofenceSagaInitializer = new DeleteGeofenceSagaInitializer(
                        User is null ? false : true,
                        User?.UserFromClaims().Email ?? "", //INSERT TOKEN HERE
                        Domain,
                        externalId,
                        projectId.GetValueOrDefault()
                    );
                    return await Task.Run(() => base.Send(deleteGeofenceSagaInitializer));
                }
                logger.LogWarning("The user was authorized for a project but the project ID was not successfully retrieved. This may be the result of a concurrency issue. Verify the project still exists.");
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
                logger.LogError(ex, "Failed to delete geofence.");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
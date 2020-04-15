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

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "User")]
    public class GeofenceController : BaseController<GeofenceController>
    {
        private readonly GeofencesHttpClient geofencesClient;
        private readonly ILogger<GeofenceController> logger;
        private readonly ProjectsHttpClient projectsClient;

        public GeofenceController(IBusPublisher busPublisher, GeofencesHttpClient geofencesClient, ProjectsHttpClient projectsClient, ILogger<GeofenceController> logger) : base(busPublisher, logger)
        {
            this.projectsClient = projectsClient;
            this.logger = logger;
            this.geofencesClient = geofencesClient;
        }

        ///<summary>
        /// Gets all geofences for a tenant's project
        ///</summary>
        ///<param name="projectName">The friendly name of the project</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/{projectName}/geofences")]
        [Authorize("BelongsToProject")]
        public async Task<ApiResponse> GetAllGeofences(string projectName)
        {
            var authorizedProject = HttpContext.Items["AuthorizedProject"] as ProjectModel;
            var apiResponse = await geofencesClient.GetAllGeofencesByProjectId<IEnumerable<GeofenceResponseModel>>(TenantId, authorizedProject.ProjectId);
            if (apiResponse.IsError)
            {
                throw new ApiException(apiResponse.ResponseException.ExceptionMessage.Error.Message, apiResponse.StatusCode);
            }
            return new ApiResponse("Successfully retrieved geofences", apiResponse.Result);
        }

        ///<summary>
        /// Initiates the creation of a new geofence within a project 
        ///</summary>
        ///<param name="projectName">The friendly name of the project</param>
        ///<param name="geofenceModel">The model necessary to create a new geofence</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("/{projectName}/geofences")]
        [Authorize("BelongsToProject")]
        public async Task<ApiResponse> Post(string projectName, GeofenceRequestModel geofenceModel)
        {
            var project = HttpContext.Items["AuthorizedProject"] as ProjectModel;
            ValidateGeofence(geofenceModel);

            var createGeofenceSagaInitializer = new CreateGeofenceSagaInitializer(
                User is null ? false : true,
                User?.UserFromClaims().Email ?? "", //INSERT TOKEN HERE
                TenantId,
                geofenceModel.ExternalId,
                project.ProjectId,
                geofenceModel.Shape,
                geofenceModel.Coordinates,
                geofenceModel.Labels,
                geofenceModel.IntegrationIds,
                geofenceModel.Metadata,
                geofenceModel.Description,
                geofenceModel.Radius,
                geofenceModel.Enabled,
                geofenceModel.OnEnter,
                geofenceModel.OnDwell,
                geofenceModel.OnExit,
                geofenceModel.ExpirationDate,
                geofenceModel.LaunchDate,
                geofenceModel.Schedule
            );
            return await Task.Run(() => base.Send(createGeofenceSagaInitializer));
        }

        private static void ValidateGeofence(GeofenceRequestModel geofenceModel)
        {
            if (geofenceModel.Shape == GeofenceShapeEnum.Circle)
            {
                if (geofenceModel.Radius < 50)
                {
                    throw new ApiException("Circular geofence radius must be greater than or equal to 50 meters.", StatusCodes.Status400BadRequest);
                }
                if (geofenceModel.Radius > 10000)
                {
                    throw new ApiException("Circular geofence radius must be less than 10000 meters.", StatusCodes.Status400BadRequest);
                }
                if (geofenceModel.Coordinates.Count() == 0 || geofenceModel.Coordinates.Count() > 1)
                {
                    throw new ApiException("Circular geofence must have exactly one coordinate in their Coordinate array.", StatusCodes.Status400BadRequest);
                }
            }
            else
            {
                if (geofenceModel.Coordinates.First().Equals(geofenceModel.Coordinates.Last()))
                {
                    throw new ApiException("The first and last coordinates in a polygon are implicitely connected. Remove the explicit closing edge.", StatusCodes.Status400BadRequest);
                }
                geofenceModel.Radius = 0;
                if (geofenceModel.Coordinates.Count() < 3)
                {
                    throw new ApiException("Polygon geofence must have three or more coordinate in their Coordinate array.", StatusCodes.Status400BadRequest);
                }
            }

            if (geofenceModel.ExpirationDate < geofenceModel.LaunchDate)
            {
                throw new ApiException("The geofence expiration date was before the launch date.", StatusCodes.Status400BadRequest);
            }
        }

        ///<summary>
        /// Updates an existing geofence within a project
        ///</summary>
        ///<param name="projectName">The friendly name of the project</param>
        ///<param name="geofenceId">The unique identifier of the geofence to update</param>
        ///<param name="geofenceModel">The model necessary to create a new geofence</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("/{projectName}/geofences/{geofenceId}")]
        [Authorize("BelongsToProject")]
        public async Task<ApiResponse> UpdateGeofence(string projectName, Guid geofenceId, GeofenceRequestModel geofenceModel)
        {
            var project = HttpContext.Items["AuthorizedProject"] as ProjectModel;
            ValidateGeofence(geofenceModel);

            var createGeofenceSagaInitializer = new UpdateGeofenceSagaInitializer(
                User is null ? false : true,
                User?.UserFromClaims().Email ?? "", //INSERT TOKEN HERE
                TenantId,
                geofenceId,
                geofenceModel.ExternalId,
                project.ProjectId,
                geofenceModel.Shape,
                geofenceModel.Coordinates,
                geofenceModel.Labels,
                geofenceModel.IntegrationIds,
                geofenceModel.Metadata,
                geofenceModel.Description,
                geofenceModel.Radius,
                geofenceModel.Enabled,
                geofenceModel.OnEnter,
                geofenceModel.OnDwell,
                geofenceModel.OnExit,
                geofenceModel.ExpirationDate,
                geofenceModel.LaunchDate,
                geofenceModel.Schedule
            );

            return await Task.Run(() => base.Send(createGeofenceSagaInitializer));
        }

        ///<summary>
        /// Deletes an existing geofence within a project
        ///</summary>
        ///<param name="projectName">The friendly name of the project</param>
        ///<param name="externalId">The friendly name of the geofence to delete</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpDelete("/{projectName}/geofences/{externalId}")]
        [Authorize("BelongsToProject")]
        public async Task<ApiResponse> DeleteGeofence(string projectName, string externalId)
        {
            var project = HttpContext.Items["AuthorizedProject"] as ProjectModel;
            var deleteGeofenceSagaInitializer = new DeleteGeofenceSagaInitializer(
                 User is null ? false : true,
                 User?.UserFromClaims().Email ?? "", //TODO: INSERT POSSIBLE TOKEN HERE
                 TenantId,
                 externalId,
                 project.ProjectId
             );
            return await Task.Run(() => base.Send(deleteGeofenceSagaInitializer));
        }
    }
}
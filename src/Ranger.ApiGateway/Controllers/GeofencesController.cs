using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [ApiController]
    [ApiVersion("1.0")]
    public class GeofencesController : BaseController<GeofencesController>
    {
        private readonly GeofencesHttpClient geofencesClient;
        private readonly ILogger<GeofencesController> logger;
        private readonly ProjectsHttpClient projectsClient;

        public GeofencesController(IBusPublisher busPublisher, GeofencesHttpClient geofencesClient, ProjectsHttpClient projectsClient, ILogger<GeofencesController> logger) : base(busPublisher, logger)
        {
            this.projectsClient = projectsClient;
            this.logger = logger;
            this.geofencesClient = geofencesClient;
        }

        ///<summary>
        /// Gets all geofences for a tenant's project
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/{projectId}/geofences")]
        [Authorize(Policy = AuthorizationPolicyNames.UserBelongsToProjectOrValidProjectApiKey)]
        public async Task<ApiResponse> GetAllGeofences(Guid projectId, CancellationToken cancellationToken)
        {
            var apiResponse = await geofencesClient.GetAllGeofencesByProjectId<IEnumerable<GeofenceResponseModel>>(TenantId, projectId, cancellationToken);
            return new ApiResponse("Successfully retrieved geofences", apiResponse.Result);
        }

        ///<summary>
        /// Initiates the creation of a new geofence within a project 
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        ///<param name="geofenceModel">The model necessary to create a new geofence</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("/{projectId}/geofences")]
        [Authorize(Policy = AuthorizationPolicyNames.UserBelongsToProjectOrValidProjectApiKey)]
        public async Task<ApiResponse> Post(Guid projectId, GeofenceRequestModel geofenceModel)
        {
            geofenceModel.ExternalId = geofenceModel.ExternalId.Trim();
            var createGeofenceSagaInitializer = new CreateGeofenceSagaInitializer(
                 User is null ? false : true,
                 User?.UserFromClaims().Email ?? HttpContext.Items[HttpContextAuthItems.ProjectApiKeyPrefix] as string,
                 TenantId,
                 geofenceModel.ExternalId,
                 projectId,
                 geofenceModel.Shape,
                 geofenceModel.Coordinates,
                 null,
                 geofenceModel.IntegrationIds,
                 geofenceModel.Metadata,
                 geofenceModel.Description,
                 geofenceModel.Radius,
                 geofenceModel.Enabled,
                 geofenceModel.OnEnter,
                 geofenceModel.OnDwell,
                 geofenceModel.OnExit,
                 null,
                 null,
                 geofenceModel.Schedule
             );
            return await Task.Run(() => base.SendAndAccept(createGeofenceSagaInitializer));
        }

        ///<summary>
        /// Updates an existing geofence within a project
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        ///<param name="geofenceId">The unique identifier of the geofence to update</param>
        ///<param name="geofenceModel">The model necessary to create a new geofence</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("/{projectId}/geofences/{geofenceId}")]
        [Authorize(Policy = AuthorizationPolicyNames.UserBelongsToProjectOrValidProjectApiKey)]
        public async Task<ApiResponse> UpdateGeofence(Guid projectId, Guid geofenceId, GeofenceRequestModel geofenceModel)
        {
            geofenceModel.ExternalId = geofenceModel.ExternalId.Trim();
            var createGeofenceSagaInitializer = new UpdateGeofenceSagaInitializer(
                User is null ? false : true,
                User?.UserFromClaims().Email ?? HttpContext.Items[HttpContextAuthItems.Project] as string,
                TenantId,
                geofenceId,
                geofenceModel.ExternalId,
                projectId,
                geofenceModel.Shape,
                geofenceModel.Coordinates,
                null,
                geofenceModel.IntegrationIds,
                geofenceModel.Metadata,
                geofenceModel.Description,
                geofenceModel.Radius,
                geofenceModel.Enabled,
                geofenceModel.OnEnter,
                geofenceModel.OnDwell,
                geofenceModel.OnExit,
                null,
                null,
                geofenceModel.Schedule
            );

            return await Task.Run(() => base.SendAndAccept(createGeofenceSagaInitializer));
        }

        ///<summary>
        /// Deletes an existing geofence within a project
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        ///<param name="externalId">The friendly name of the geofence to delete</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpDelete("/{projectId}/geofences/{externalId}")]
        [Authorize(Policy = AuthorizationPolicyNames.UserBelongsToProjectOrValidProjectApiKey)]
        public async Task<ApiResponse> DeleteGeofence(Guid projectId, string externalId)
        {
            var deleteGeofenceSagaInitializer = new DeleteGeofenceSagaInitializer(
                 User is null ? false : true,
                 UserFromClaims.Email ?? HttpContext.Items[HttpContextAuthItems.ProjectApiKeyPrefix] as string,
                 TenantId,
                 externalId,
                 projectId
             );
            return await Task.Run(() => base.SendAndAccept(deleteGeofenceSagaInitializer));
        }
    }
}
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
    public class GeofencesUserController : GeofencesBaseController
    {
        public GeofencesUserController(IBusPublisher busPublisher, GeofencesHttpClient geofencesClient, ProjectsHttpClient projectsClient, ILogger<GeofencesUserController> logger)
         : base(busPublisher, geofencesClient, projectsClient, logger)
        { }

        ///<summary>
        /// Gets all geofences for a tenant's project
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/{projectId}/geofences")]
        [Authorize(Policy = AuthorizationPolicyNames.BelongsToProject)]
        public async Task<ApiResponse> GetAllGeofencesForUser(Guid projectId, CancellationToken cancellationToken)
        {
            return await base.GetAllGeofences(projectId, cancellationToken);
        }

        ///<summary>
        /// Initiates the creation of a new geofence within a project 
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        ///<param name="geofenceModel">The model necessary to create a new geofence</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("/{projectId}/geofences")]
        [Authorize(Policy = AuthorizationPolicyNames.BelongsToProject)]
        public async Task<ApiResponse> Post(Guid projectId, GeofenceRequestModel geofenceModel)
        {
            return await base.Post(projectId, true, User.UserFromClaims().Email, geofenceModel);
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
        [Authorize(Policy = AuthorizationPolicyNames.BelongsToProject)]
        public async Task<ApiResponse> UpdateGeofence(Guid projectId, Guid geofenceId, GeofenceRequestModel geofenceModel)
        {
            return await base.UpdateGeofence(projectId, true, User.UserFromClaims().Email, geofenceId, geofenceModel);
        }

        ///<summary>
        /// Deletes an existing geofence within a project
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        ///<param name="externalId">The friendly name of the geofence to delete</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpDelete("/{projectId}/geofences/{externalId}")]
        [Authorize(Policy = AuthorizationPolicyNames.BelongsToProject)]
        public async Task<ApiResponse> DeleteGeofence(Guid projectId, string externalId)
        {
            return await base.DeleteGeofence(projectId, true, User.UserFromClaims().Email, externalId);
        }
    }
}
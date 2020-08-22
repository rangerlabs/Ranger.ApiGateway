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
    public class GeofencesProjectApiKeyController : GeofencesBaseController
    {
        public GeofencesProjectApiKeyController(IBusPublisher busPublisher, GeofencesHttpClient geofencesClient, ProjectsHttpClient projectsClient, ILogger<GeofencesProjectApiKeyController> logger)
        : base(busPublisher, geofencesClient, projectsClient, logger)
        { }

        ///<summary>
        /// Gets all geofences for a tenant's project
        ///</summary>
        /// <param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/geofences")]
        [Authorize(Policy = AuthorizationPolicyNames.ValidProjectApiKey)]
        public async Task<ApiResponse> GetAllGeofencesForProjectApiKey(CancellationToken cancellationToken)
        {
            return await base.GetAllGeofences(ProjectId, cancellationToken);
        }

        ///<summary>
        /// Initiates the creation of a new geofence within a project 
        ///</summary>
        ///<param name="geofenceModel">The model necessary to create a new geofence</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("/geofences")]
        [Authorize(Policy = AuthorizationPolicyNames.ValidProjectApiKey)]
        public async Task<ApiResponse> Post(GeofenceRequestModel geofenceModel)
        {
            return await base.Post(ProjectId, false, ApiKeyPrefix, geofenceModel);
        }

        ///<summary>
        /// Updates an existing geofence within a project
        ///</summary>
        ///<param name="geofenceId">The unique identifier of the geofence to update</param>
        ///<param name="geofenceModel">The model necessary to create a new geofence</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("/geofences/{geofenceId}")]
        [Authorize(Policy = AuthorizationPolicyNames.ValidProjectApiKey)]
        public async Task<ApiResponse> UpdateGeofence(Guid geofenceId, GeofenceRequestModel geofenceModel)
        {
            return await base.UpdateGeofence(ProjectId, false, ApiKeyPrefix, geofenceId, geofenceModel);
        }

        ///<summary>
        /// Deletes an existing geofence within a project
        ///</summary>
        ///<param name="externalId">The friendly name of the geofence to delete</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpDelete("/geofences/{externalId}")]
        [Authorize(Policy = AuthorizationPolicyNames.ValidProjectApiKey)]
        public async Task<ApiResponse> DeleteGeofence(string externalId)
        {
            return await base.DeleteGeofence(ProjectId, false, ApiKeyPrefix, externalId);
        }

        private Guid ProjectId => (HttpContext.Items[HttpContextAuthItems.Project] as ProjectModel).Id;
        private string ApiKeyPrefix => HttpContext.Items[HttpContextAuthItems.ProjectApiKeyPrefix] as string;
    }
}
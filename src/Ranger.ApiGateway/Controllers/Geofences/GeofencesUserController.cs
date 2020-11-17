using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.ApiUtilities;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.Services.Geofences;

namespace Ranger.ApiGateway
{
    [ApiController]
    [ApiVersion("1.0")]
    public class GeofencesUserController : GeofencesBaseController
    {
        private readonly IValidator<GeofenceRequestParams> paramValidator;

        public GeofencesUserController(
            IBusPublisher busPublisher,
            IGeofencesHttpClient geofencesClient,
            IProjectsHttpClient projectsClient,
            IValidator<GeofenceRequestParams> paramValidator,
            ILogger<GeofencesUserController> logger)
         : base(busPublisher, geofencesClient, projectsClient, logger)
        {
            this.paramValidator = paramValidator;
        }

        ///<summary>
        /// Gets geofences for a tenant's project
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        /// <param name="orderBy">The field to order by. One of ExternalId, Shape, Enabled, CreatedDate, UpdatedDate. Defaults to CreatedDate.</param>
        /// <param name="sortOrder">The order to sort by. One of Asc or Desc. Defaults to Desc.</param>
        /// <param name="page">The page to return. Defaults to 1.</param>
        /// <param name="pageCount">The number of geofences per page. Defaults to 100. Less than or equal to 1000</param>
        /// <param name="bounds">The bounding rectangle to retrieve geofences within</param>
        /// <param name="cancellationToken"></param>
        /// <param name="externalId">The external id to query for</param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/{projectId}/geofences")]
        [Authorize(Policy = AuthorizationPolicyNames.BelongsToProject)]
        public async Task<ApiResponse> GetGeofencesForUser(
            Guid projectId,
            CancellationToken cancellationToken,
            [FromQuery] string externalId = null,
            [FromQuery] string orderBy = OrderByOptions.CreatedDateLowerInvariant,
            [FromQuery] string sortOrder = GeofenceSortOrders.DescendingLowerInvariant,
            [FromQuery] int page = 0,
            [FromQuery] int pageCount = 100,
            [FromQuery] [ModelBinder(typeof(SemicolonDelimitedLngLatArrayModelBinder))] IEnumerable<LngLat> bounds = null)
        {
            var validationResult = paramValidator.Validate(new GeofenceRequestParams(externalId, sortOrder, orderBy, page, pageCount, bounds), options => options.IncludeRuleSets("Get"));
            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors.Select(f => new ValidationError(f.PropertyName, f.ErrorMessage));
                throw new ApiException(validationErrors);
            }
                
            return await base.GetGeofences(projectId, externalId, orderBy, sortOrder, page, pageCount, bounds, cancellationToken);
       }

        ///<summary>
        /// Gets geofence count for a tenant's project
        ///</summary>
        ///<param name="projectId">The unique identifier of the project</param>
        ///<param name="cancellationToken"></param>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("/{projectId}/geofences/count")]
        [Authorize(Policy = AuthorizationPolicyNames.BelongsToProject)]
        public async Task<ApiResponse> GetGeofenceCountForProject(
            Guid projectId,
            CancellationToken cancellationToken)
        {
            return await base.GetGeofenceCount(projectId, cancellationToken);
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
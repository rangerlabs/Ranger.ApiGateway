using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.ApiGateway.Messages.Commands;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.ApiGateway
{
    public class GeofencesBaseController : BaseController
    {
        private readonly IGeofencesHttpClient geofencesClient;
        private readonly ILogger logger;
        private readonly IProjectsHttpClient projectsClient;

        protected GeofencesBaseController(IBusPublisher busPublisher, IGeofencesHttpClient geofencesClient, IProjectsHttpClient projectsClient, ILogger logger) : base(busPublisher, logger)
        {
            this.projectsClient = projectsClient;
            this.logger = logger;
            this.geofencesClient = geofencesClient;
        }

        protected async Task<ApiResponse> GetGeofences(Guid projectId, string externalId, string orderBy, string sortOrder, int page, int pageCount, string search, IEnumerable<LngLat> bounds, CancellationToken cancellationToken)
        {
            if (!(externalId is null))
            {
                var apiResponse = await geofencesClient.GetGeofenceByExternalId<GeofenceResponseModel>(TenantId, projectId, externalId, cancellationToken);
                return new ApiResponse("Successfully retrieved geofence", apiResponse.Result);
            }
            if (!(bounds is null))
            {
                var apiResponse = await geofencesClient.GetGeofencesByBounds<IEnumerable<GeofenceResponseModel>>(TenantId, projectId, orderBy, sortOrder, bounds, cancellationToken);
                return new ApiResponse("Successfully retrieved bounded geofences", apiResponse.Result);
            }
            else
            {
                var apiResponse = await geofencesClient.GetGeofencesByProjectId<IEnumerable<GeofenceResponseModel>>(TenantId, projectId, orderBy, sortOrder, page, pageCount, search, cancellationToken);
                Response.Headers.Add("X-Pagination-TotalCount", apiResponse.Headers.GetValues("X-Pagination-TotalCount").ElementAt(0));
                Response.Headers.Add("X-Pagination-Page", apiResponse.Headers.GetValues("X-Pagination-Page").ElementAt(0));
                Response.Headers.Add("X-Pagination-PageCount", apiResponse.Headers.GetValues("X-Pagination-PageCount").ElementAt(0));
                Response.Headers.Add("X-Pagination-OrderBy", apiResponse.Headers.GetValues("X-Pagination-OrderBy").ElementAt(0));
                Response.Headers.Add("X-Pagination-SortOrder", apiResponse.Headers.GetValues("X-Pagination-SortOrder").ElementAt(0));
                return new ApiResponse("Successfully retrieved paginated geofences", apiResponse.Result);
            }
        }

        protected async Task<ApiResponse> BulkDeleteGeofences(Guid projectId, bool isFrontendRequest, string commandingUser, IEnumerable<string> externalIds)
        {
            var bulkDeleteGeofences = new BulkDeleteGeofencesSagaInitializer(isFrontendRequest, commandingUser, TenantId, projectId, externalIds);
            return await Task.Run(() => base.SendAndAccept(bulkDeleteGeofences));
        }

        protected async Task<ApiResponse> GetGeofenceCount(Guid projectId, CancellationToken cancellationToken)
        {
            var apiResponse = await geofencesClient.GetGeofencesCountForProject(TenantId, projectId, cancellationToken);
            return new ApiResponse("Successfully retrieved geofence count for project", apiResponse.Result);
        }

        protected async Task<ApiResponse> Post(Guid projectId, bool isFrontendRequest, string commandingUser, GeofenceRequestModel geofenceModel)
        {
            var radius = getRadiusOrDefaultRadius(geofenceModel);
            geofenceModel.ExternalId = geofenceModel.ExternalId.Trim();
            var createGeofenceSagaInitializer = new CreateGeofenceSagaInitializer(
                isFrontendRequest,
                commandingUser,
                TenantId,
                geofenceModel.ExternalId,
                projectId,
                geofenceModel.Shape,
                geofenceModel.Coordinates,
                null,
                geofenceModel.IntegrationIds,
                geofenceModel.Metadata,
                geofenceModel.Description,
                radius,
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

        protected async Task<ApiResponse> UpdateGeofence(Guid projectId, bool isFrontendRequest, string commandingUser, Guid geofenceId, GeofenceRequestModel geofenceModel)
        {
            var radius = getRadiusOrDefaultRadius(geofenceModel);
            geofenceModel.ExternalId = geofenceModel.ExternalId.Trim();
            var createGeofenceSagaInitializer = new UpdateGeofenceSagaInitializer(
                isFrontendRequest,
                commandingUser,
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
                radius,
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

        protected async Task<ApiResponse> DeleteGeofence(Guid projectId, bool isFrontendRequest, string commandingUser, string externalId)
        {
            var deleteGeofenceSagaInitializer = new DeleteGeofenceSagaInitializer(
                isFrontendRequest,
                commandingUser,
                TenantId,
                externalId,
                projectId
             );
            return await Task.Run(() => base.SendAndAccept(deleteGeofenceSagaInitializer));
        }

        private int getRadiusOrDefaultRadius(GeofenceRequestModel geofenceRequestModel)
        {
            if (geofenceRequestModel.Shape is GeofenceShapeEnum.Circle)
            {
                if (geofenceRequestModel.Radius == 0)
                {
                    return 100;
                }
                else
                {
                    return geofenceRequestModel.Radius;
                }
            }
            else
            {
                return 0;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.Common;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.ApiGateway
{
    public class GeofencesBaseController : BaseController
    {
        private readonly GeofencesHttpClient geofencesClient;
        private readonly ILogger logger;
        private readonly ProjectsHttpClient projectsClient;

        protected GeofencesBaseController(IBusPublisher busPublisher, GeofencesHttpClient geofencesClient, ProjectsHttpClient projectsClient, ILogger logger) : base(busPublisher, logger)
        {
            this.projectsClient = projectsClient;
            this.logger = logger;
            this.geofencesClient = geofencesClient;
        }

        protected async Task<ApiResponse> GetAllGeofences(Guid projectId, CancellationToken cancellationToken)
        {
            var apiResponse = await geofencesClient.GetAllGeofencesByProjectId<IEnumerable<GeofenceResponseModel>>(TenantId, projectId, cancellationToken);
            return new ApiResponse("Successfully retrieved geofences", apiResponse.Result);
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
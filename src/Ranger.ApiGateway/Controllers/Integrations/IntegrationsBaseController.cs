using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;

namespace Ranger.ApiGateway
{
    public class IntegrationsBaseController : BaseController
    {
        private readonly IIntegrationsHttpClient integrationsClient;
        private readonly ILogger logger;
        private readonly IProjectsHttpClient projectsClient;

        protected IntegrationsBaseController(IBusPublisher busPublisher, IIntegrationsHttpClient integrationsClient, IProjectsHttpClient projectsClient, ILogger logger) : base(busPublisher, logger)
        {
            this.projectsClient = projectsClient;
            this.logger = logger;
            this.integrationsClient = integrationsClient;
        }

        protected async Task<ApiResponse> DeleteIntegrationForProject(Guid projectId, string integrationName)
        {
            return await Task.Run(() => base.SendAndAccept(new DeleteIntegrationSagaInitializer(UserFromClaims.Email, TenantId, integrationName, projectId)));
        }

        protected async Task<ApiResponse> GetAllIntegrationsForProject(Guid projectId, CancellationToken cancellationToken)
        {
            var integrationsApiResponse = await integrationsClient.GetAllIntegrationsByProjectId<IEnumerable<dynamic>>(TenantId, projectId, cancellationToken);
            return new ApiResponse("Succesfully retrieved integrations", integrationsApiResponse.Result);
        }
    }
}
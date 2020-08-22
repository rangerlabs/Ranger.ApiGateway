using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    public class IntegrationsBaseController : BaseController
    {
        private readonly IntegrationsHttpClient integrationsClient;
        private readonly ILogger logger;
        private readonly ProjectsHttpClient projectsClient;

        protected IntegrationsBaseController(IBusPublisher busPublisher, IntegrationsHttpClient integrationsClient, ProjectsHttpClient projectsClient, ILogger logger) : base(busPublisher, logger)
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
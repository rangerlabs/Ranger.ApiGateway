using System;
using System.Linq;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway
{
    public partial class ValidBreadcrumbApiKeyHandler : AuthorizationHandler<ValidBreadcrumbApiKeyRequirement>
    {
        private readonly ITenantsHttpClient tenantsClient;
        private readonly IProjectsHttpClient projectsClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<ValidBreadcrumbApiKeyHandler> logger;
        public ValidBreadcrumbApiKeyHandler(ITenantsHttpClient tenantsClient, IProjectsHttpClient projectsClient, IHttpContextAccessor httpContextAccessor, ILogger<ValidBreadcrumbApiKeyHandler> logger)
        {
            this.tenantsClient = tenantsClient;
            this.projectsClient = projectsClient;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ValidBreadcrumbApiKeyRequirement requirement)
        {
            try
            {
                StringValues apiKey;
                var apiKeySuccess = httpContextAccessor.HttpContext.Request.Headers.TryGetValue("x-ranger-apikey", out apiKey);
                if (apiKeySuccess)
                {
                    if (apiKey.Count == 1)
                    {
                        var apiKeyParts = apiKey[0].Split('.');
                        if (apiKeyParts?.Length == 2 && apiKeyParts[0] != "live" && apiKeyParts[0] != "test")
                        {
                            logger.LogInformation($"The API key provided was for the incorrect purpose");
                        }
                        else if (apiKeyParts?.Length == 2 && (apiKeyParts[0] == "live" || apiKeyParts[0] == "test") && Guid.TryParse(apiKeyParts[1], out _))
                        {
                            var tenantIdResponse = await projectsClient.GetTenantIdByApiKeyAsync(apiKey.Single());
                            logger.LogInformation("Resolved Tenant Id {TenantId} from API key");

                            // this will throw if the tenant was removed
                            await tenantsClient.GetTenantByIdAsync<NewTenantPostModel>(tenantIdResponse.Result);
                            logger.LogInformation("Tenant Id {TenantId} was found to be active");

                            var projectApiResponse = await projectsClient.GetProjectByApiKeyAsync<ProjectModel>(tenantIdResponse.Result, apiKey);
                            if (projectApiResponse.Result.Enabled)
                            {
                                httpContextAccessor.HttpContext.Items[HttpContextAuthItems.BreadcrumbApiKeyEnvironment] = apiKeyParts[0].ToUpperInvariant();
                                httpContextAccessor.HttpContext.Items[HttpContextAuthItems.TenantId] = tenantIdResponse.Result;
                                httpContextAccessor.HttpContext.Items[HttpContextAuthItems.Project] = projectApiResponse.Result;
                                logger.LogInformation("Authorization succeeded for policy 'ValidBreadcrumbApiKeyHandler");
                                context.Succeed(requirement);
                            }
                            else
                            {
                                logger.LogInformation("Project {Id} is not enabled for tenant id {TenantId}", projectApiResponse.StatusCode, projectApiResponse.Result.Id, tenantIdResponse.Result);
                                context.Fail();
                            }
                        }
                        else
                        {
                            logger.LogInformation($"The API key was not a valid format");
                        }
                    }
                    else
                    {
                        logger.LogInformation("Multiple x-ranger-apikey headers were present in the request");
                    }
                }
                else
                {
                    logger.LogInformation("No x-ranger-apikey header was present in the request");
                }
            }
            catch (ApiException ex)
            {
                logger.LogInformation(ex, "Failed to retrieve a resource necessary to validate the requirement");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An exception occurred validating the Breadcrumb Api Key");
            }
        }
    }
}
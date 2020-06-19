using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway
{
    public class ValidProjectApiKeyHandler : AuthorizationHandler<UserBelongsToProjectOrValidProjectApiKeyRequirement>
    {
        private readonly TenantsHttpClient tenantsClient;
        private readonly ProjectsHttpClient projectsClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<ValidProjectApiKeyHandler> logger;
        public ValidProjectApiKeyHandler(TenantsHttpClient tenantsClient, ProjectsHttpClient projectsClient, IHttpContextAccessor httpContextAccessor, ILogger<ValidProjectApiKeyHandler> logger)
        {
            this.tenantsClient = tenantsClient;
            this.projectsClient = projectsClient;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserBelongsToProjectOrValidProjectApiKeyRequirement requirement)
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
                        if (apiKeyParts?.Length == 2 && (apiKeyParts[0] == "live" || apiKeyParts[0] == "test"))
                        {
                            logger.LogInformation($"The API key provided was for the incorrect purpose");
                        }

                        if (apiKeyParts?.Length == 2 && apiKeyParts[0] == "proj" && Guid.TryParse(apiKeyParts[1], out _))
                        {
                            var apiResponse = await projectsClient.GetTenantIdByApiKeyAsync(apiKey.Single());
                            if (!apiResponse.IsError)
                            {
                                var tenantId = await TenantIdResolver<ValidProjectApiKeyHandler>.ResolveTenantId(httpContextAccessor.HttpContext, tenantsClient, apiResponse.Result, logger);
                                if (String.IsNullOrWhiteSpace(tenantId))
                                {
                                    context.Fail();
                                }
                                else
                                {
                                    var projectApiResponse = await projectsClient.GetProjectByApiKeyAsync<ProjectAuthenticationResult>(tenantId, apiKey);
                                    if (!projectApiResponse.IsError)
                                    {
                                        if (projectApiResponse.Result.Enabled)
                                        {
                                            httpContextAccessor.HttpContext.Items[HttpContextAuthItems.ProjectApiKeyPrefix] = apiKey.Single().Substring(0, 11);
                                            httpContextAccessor.HttpContext.Items[HttpContextAuthItems.TenantId] = tenantId;
                                            httpContextAccessor.HttpContext.Items[HttpContextAuthItems.Project] = projectApiResponse.Result;
                                            logger.LogInformation("Authorization succeeded for policy 'ValidProjectApiKeyHandler");
                                            context.Succeed(requirement);
                                        }
                                        else
                                        {
                                            logger.LogInformation("Project {Id} is not enabled for tenant id {TenantId}", projectApiResponse.StatusCode, projectApiResponse.Result.ProjectId, tenantId);
                                            context.Fail();
                                        }
                                    }
                                    else
                                    {
                                        logger.LogInformation("Received {Status} when attempting to retrieve project for tenant id {TenantId} and api key in environment {Environment}", projectApiResponse.StatusCode, tenantId, apiKeyParts[0].ToUpperInvariant());
                                    }
                                }
                            }
                            else
                            {
                                logger.LogInformation("Received {Status} when attempting to retrieve tenant id for api key in environment {Environment}", apiResponse.StatusCode, apiKeyParts[0].ToUpperInvariant());
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
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An exception occurred validating the Project Api Key");
            }
        }
    }
}
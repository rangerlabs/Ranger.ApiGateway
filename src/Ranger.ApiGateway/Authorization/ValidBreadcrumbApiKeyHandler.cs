using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway.Authorization
{
    public class ValidBreadcrumbApiKeyHandler : AuthorizationHandler<ValidBreadcrumbApiKeyRequirement>
    {
        private readonly TenantsHttpClient tenantsClient;
        private readonly ProjectsHttpClient projectsClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<ValidBreadcrumbApiKeyHandler> logger;
        public ValidBreadcrumbApiKeyHandler(TenantsHttpClient tenantsClient, ProjectsHttpClient projectsClient, IHttpContextAccessor httpContextAccessor, ILogger<ValidBreadcrumbApiKeyHandler> logger)
        {
            this.tenantsClient = tenantsClient;
            this.projectsClient = projectsClient;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        private class ProjectAuthenticationResult
        {
            public Guid ProjectId { get; set; }
            public string Name { get; set; }
            public bool Enabled { get; set; }
        }
        private class TenantResult
        {
            public string TenantId { get; set; }
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
                        if (apiKeyParts?.Length == 2 && apiKeyParts[0] == "proj")
                        {
                            logger.LogInformation($"The API key provided was for the incorrect purpose");
                            context.Fail();
                        }

                        if (apiKeyParts?.Length == 2 && (apiKeyParts[0] == "live" || apiKeyParts[0] == "test") && Guid.TryParse(apiKeyParts[1], out _))
                        {
                            var apiResponse = await projectsClient.GetTenantIdByApiKeyAsync(apiKey.Single());
                            if (!apiResponse.IsError)
                            {
                                var tenantApiResponse = await tenantsClient.GetTenantByIdAsync<TenantResult>(apiResponse.Result);
                                if (!tenantApiResponse.IsError)
                                {
                                    var projectApiResponse = await projectsClient.GetProjectByApiKeyAsync<ProjectAuthenticationResult>(tenantApiResponse.Result.TenantId, apiKey);
                                    if (!projectApiResponse.IsError)
                                    {
                                        if (projectApiResponse.Result.Enabled)
                                        {
                                            httpContextAccessor.HttpContext.Items["ApiKeyEnvironment"] = apiKeyParts[0].ToUpperInvariant();
                                            httpContextAccessor.HttpContext.Items["TenantId"] = tenantApiResponse.Result.TenantId;
                                            httpContextAccessor.HttpContext.Items["ProjectId"] = projectApiResponse.Result.ProjectId.ToString();
                                            httpContextAccessor.HttpContext.Items["ProjectName"] = projectApiResponse.Result.Name.ToString();
                                            context.Succeed(requirement);
                                        }
                                        else
                                        {
                                            logger.LogInformation("Project {Id} is not enabled for tenant id {TenantId}", projectApiResponse.StatusCode, projectApiResponse.Result.ProjectId, tenantApiResponse.Result.TenantId);
                                            context.Fail();
                                        }
                                    }
                                    else
                                    {
                                        logger.LogInformation("Received {Status} when attempting to retrieve project for tenant id {TenantId} and api key in environment {Environment}", projectApiResponse.StatusCode, tenantApiResponse.Result, apiKeyParts[0].ToUpperInvariant());
                                        context.Fail();
                                    }
                                }
                                else
                                {
                                    logger.LogInformation("Received {Status} when attempting to retrieve tenant for tenant id {TenantId}", tenantApiResponse.StatusCode, tenantApiResponse.Result);
                                    context.Fail();
                                }
                            }
                            else
                            {
                                logger.LogInformation("Received {Status} when attempting to retrieve tenant id for api key in environment {Environment}", apiResponse.StatusCode, apiKeyParts[0].ToUpperInvariant());
                                context.Fail();
                            }
                        }
                        else
                        {
                            logger.LogInformation($"The API key was not a valid format");
                            context.Fail();
                        }
                    }
                    else
                    {
                        logger.LogInformation("Multiple x-ranger-apikey headers were present in the request");
                        context.Fail();
                    }
                }
                else
                {
                    logger.LogInformation("No x-ranger-apikey header was present in the request");
                    context.Fail();
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An exception occurred validating the Breadcrumb Api Key");
            }
        }
    }
}
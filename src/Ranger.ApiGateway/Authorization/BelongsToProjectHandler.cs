using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway.Authorization
{
    public class BelongsToProjectHandler : AuthorizationHandler<BelongsToProjectRequirement>
    {
        private readonly ProjectsHttpClient projectsClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<BelongsToProjectHandler> logger;
        private readonly TenantsHttpClient tenantsHttpClient;
        public BelongsToProjectHandler(ProjectsHttpClient projectsClient, TenantsHttpClient tenantsHttpClient, IHttpContextAccessor httpContextAccessor, ILogger<BelongsToProjectHandler> logger)
        {
            this.tenantsHttpClient = tenantsHttpClient;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.projectsClient = projectsClient;
        }

        private class TenantResult
        {
            public string TenantId { get; set; }
            public bool Confirmed { get; set; }
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BelongsToProjectRequirement requirement)
        {
            try
            {
                var projectName = httpContextAccessor.HttpContext.Request.Path.Value.Split("/")[1];
                if (!String.IsNullOrWhiteSpace(projectName))
                {
                    var user = context.User.UserFromClaims();
                    if (!String.IsNullOrWhiteSpace(user.Domain) && !String.IsNullOrWhiteSpace(user.Email) && !String.IsNullOrWhiteSpace(user.Role))
                    {
                        var tenantApiResponse = await tenantsHttpClient.GetTenantByDomainAsync<TenantResult>(user.Domain);
                        if (!tenantApiResponse.IsError)
                        {
                            if (tenantApiResponse.Result.Confirmed)
                            {
                                httpContextAccessor.HttpContext.Items["TenantId"] = tenantApiResponse.Result.TenantId;
                                var apiResponse = await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(tenantApiResponse.Result.TenantId, user.Email).ConfigureAwait(false);
                                if (apiResponse.IsError)
                                {
                                    logger.LogError($"Failed to retrieve authorized projects to validate user's project authorization. Domain: '{user.Domain}', Email: '{user.Email}'");
                                }
                                else
                                {
                                    var project = apiResponse.Result.Where(_ => _.Name == projectName).SingleOrDefault();
                                    if (project is null)
                                    {
                                        logger.LogInformation("The user is not authorized to access the requested project");
                                    }
                                    httpContextAccessor.HttpContext.Items["AuthorizedProject"] = project;
                                    logger.LogInformation("Authorization succeeded for policy 'BelongsToProjectHandler");
                                    context.Succeed(requirement);
                                }
                            }
                            else
                            {
                                logger.LogInformation("The tenant is not yet confirmed");
                            }
                        }
                        else
                        {
                            logger.LogInformation("Received {Status} when attempting to retrieve tenant for domain {Domain}", tenantApiResponse.StatusCode, user.Domain);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "An exception occurred validating whether the user belongs to the project");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway
{

    public partial class UserBelongsToProjectHandler : AuthorizationHandler<UserBelongsToProjectOrValidProjectApiKeyRequirement>
    {
        private readonly ProjectsHttpClient projectsClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<UserBelongsToProjectHandler> logger;
        private readonly TenantsHttpClient tenantsHttpClient;

        public UserBelongsToProjectHandler(ProjectsHttpClient projectsClient, TenantsHttpClient tenantsHttpClient, IHttpContextAccessor httpContextAccessor, ILogger<UserBelongsToProjectHandler> logger)
        {
            this.tenantsHttpClient = tenantsHttpClient;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.projectsClient = projectsClient;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserBelongsToProjectOrValidProjectApiKeyRequirement requirement)
        {
            try
            {
                var projectName = httpContextAccessor.HttpContext.Request.Path.Value.Split("/")[1];
                if (!String.IsNullOrWhiteSpace(projectName))
                {
                    var user = context.User.UserFromClaims();
                    if (!String.IsNullOrWhiteSpace(user.Domain) && !String.IsNullOrWhiteSpace(user.Email) && !String.IsNullOrWhiteSpace(user.Role))
                    {
                        var tenantId = await TenantIdResolver<UserBelongsToProjectHandler>.ResolveTenantId(httpContextAccessor.HttpContext, tenantsHttpClient, user.Domain, logger);
                        if (String.IsNullOrWhiteSpace(tenantId))
                        {
                            context.Fail();
                        }
                        else
                        {
                            var apiResponse = await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(tenantId, user.Email).ConfigureAwait(false);
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
                                httpContextAccessor.HttpContext.Items[HttpContextAuthItems.TenantId] = tenantId;
                                httpContextAccessor.HttpContext.Items[HttpContextAuthItems.Project] = project;
                                logger.LogInformation("Authorization succeeded for policy 'BelongsToProjectHandler");
                                context.Succeed(requirement);
                            }
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
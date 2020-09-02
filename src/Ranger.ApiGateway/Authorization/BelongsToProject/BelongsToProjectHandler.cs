using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ranger.InternalHttpClient;

namespace Ranger.ApiGateway
{

    public partial class BelongsToProjectHandler : AuthorizationHandler<BelongsToProjectRequirement>
    {
        private readonly IProjectsHttpClient projectsClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<BelongsToProjectHandler> logger;
        private readonly ITenantsHttpClient tenantsHttpClient;

        public BelongsToProjectHandler(IProjectsHttpClient projectsClient, ITenantsHttpClient tenantsHttpClient, IHttpContextAccessor httpContextAccessor, ILogger<BelongsToProjectHandler> logger)
        {
            this.tenantsHttpClient = tenantsHttpClient;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.projectsClient = projectsClient;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BelongsToProjectRequirement requirement)
        {
            try
            {
                var requestProjectId = httpContextAccessor.HttpContext.Request.Path.Value.Split("/")[1];
                Guid projectId;
                if (Guid.TryParse(requestProjectId, out projectId))
                {
                    var user = context.User.UserFromClaims();
                    if (!String.IsNullOrWhiteSpace(user.Domain) && !String.IsNullOrWhiteSpace(user.Email) && !String.IsNullOrWhiteSpace(user.Role))
                    {
                        var tenantId = await TenantIdResolver<BelongsToProjectHandler>.ResolveTenantId(httpContextAccessor.HttpContext, tenantsHttpClient, user.Domain, logger);
                        if (String.IsNullOrWhiteSpace(tenantId))
                        {
                            context.Fail();
                        }
                        else
                        {
                            var apiResponse = await projectsClient.GetAllProjectsForUserAsync<IEnumerable<ProjectModel>>(tenantId, user.Email).ConfigureAwait(false);
                            var project = apiResponse.Result.Where(_ => _.Id == projectId).SingleOrDefault();
                            if (project is null)
                            {
                                logger.LogInformation("The user is not authorized to access the requested project");
                                context.Fail();
                            }
                            else
                            {

                                httpContextAccessor.HttpContext.Items[HttpContextAuthItems.TenantId] = tenantId;
                                httpContextAccessor.HttpContext.Items[HttpContextAuthItems.Project] = project;
                                logger.LogInformation("Authorization succeeded for policy 'BelongsToProjectHandler");
                                context.Succeed(requirement);
                            }
                        }
                    }
                }
            }
            catch (ApiException ex)
            {
                logger.LogInformation(ex, "Failed to retrieve a resource necessary to validate the requirement");
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex, "An exception occurred validating whether the user belongs to the project");
            }
        }


    }
}